

using AttributeLibrary.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitFramework;
using RabbitFramework.Contracts;
using RabbitFramework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AttributeLibrary
{
    public class RabbitInitializer
    {
        private ILogger _logger { get; } = RabbitLogging.CreateLogger<RabbitInitializer>();
        private readonly IBusProvider _busProvider;
        private readonly IServiceProvider _serviceProvider;

        public RabbitInitializer(IBusProvider busProvider, IServiceProvider serviceProvider)
        {
            _busProvider = busProvider;
            _serviceProvider = serviceProvider;
        }

        public void Initialize(Assembly executingAssembly)
        {
            _logger.LogInformation("Initializing event bus");
            _busProvider.CreateConnection();

            var types = executingAssembly.GetTypes();
            InitializeQueueListeners(types);
            _logger.LogInformation($"Initialization completed. Now listening...");
        }

        private void InitializeQueueListeners(Type[] types)
        {
            _logger.LogInformation("Initializing event listeners");
            foreach (var type in types)
            {
                var queueAttribute = type.GetCustomAttribute<QueueListenerAttribute>();

                if (queueAttribute != null)
                {
                    if (TypeContainsBothCommandsAndEvents(type))
                    {
                        throw new InvalidOperationException("Type {} can't contain both events and commands. Events and commands should not be sent to the same queue.");
                    }

                    var methods = type.GetMethods();

                    if (methods.Any(m => m.GetCustomAttributes<TopicAttribute>().Count() > 0))
                    {
                        SetUpTopicMethods(type, queueAttribute.QueueName);
                        _logger.LogInformation($"Initializing event {type}");
                    }
                    else if (methods.Any(m => m.GetCustomAttributes<CommandAttribute>().Count() > 0))
                    {
                        SetUpCommandMethods(type, queueAttribute.QueueName);
                        _logger.LogInformation($"Initializing commands {type}");
                    }
                }
            }
        }

        private bool TypeContainsBothCommandsAndEvents(Type type)
        {
            var methods = type.GetMethods();

            return
                methods.Any(m => m.GetCustomAttributes<TopicAttribute>().Count() > 0) &&
                methods.Any(m => m.GetCustomAttributes<CommandAttribute>().Count() > 0);
        }

        private void SetUpCommandMethods(Type type, string queueName)
        {
            Dictionary<string, MethodInfo> commandsWithMethods = GetCommandsWithMethods(type);
            _busProvider.SetupRpcListeners(queueName, commandsWithMethods.Keys.ToArray(), CreateCommandReceivedCallback(type, commandsWithMethods));
        }

        private void SetUpTopicMethods(Type type, string queueName)
        {
            Dictionary<string, MethodInfo> topicsWithMethods = GetTopicsWithMethods(type);
            _busProvider.CreateTopicsForQueue(queueName, topicsWithMethods.Keys.ToArray());
            var callback = CreateEventReceivedCallback(type, topicsWithMethods);
            _busProvider.BasicConsume(queueName, callback);
        }

        public Dictionary<string, MethodInfo> GetCommandsWithMethods(Type type)
        {
            return GetAttributeValuesWithMethod<CommandAttribute>(type, (a) => a.Key);
        }

        public Dictionary<string, MethodInfo> GetTopicsWithMethods(Type type)
        {
            return GetAttributeValuesWithMethod<TopicAttribute>(type, (a) => a.Topic);
        }

        private Dictionary<string, MethodInfo> GetAttributeValuesWithMethod<TAttribute>(Type type, Func<TAttribute, string> predicate) where TAttribute : Attribute
        {
            return type.GetMethods()
                .Where(m => m.GetCustomAttribute<TAttribute>() != null)
                .ToDictionary(m => predicate(m.GetCustomAttribute<TAttribute>()), m => m);
        }

        public EventReceivedCallback CreateEventReceivedCallback(Type type, Dictionary<string, MethodInfo> topics)
        {
            return (message) =>
            {
                var instance = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, type);

                var topicMatches = GetTopicMatches(message.RoutingKey, topics);

                foreach (var topic in topicMatches)
                {
                    InvokeTopic(message, instance, topic);
                }
            };
        }

        public CommandReceivedCallback CreateCommandReceivedCallback(Type type, Dictionary<string, MethodInfo> commands)
        {
            return (message) =>
            {
                var instance = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, type);

                (string name, MethodInfo method) = GetCommandMatch(message.RoutingKey, commands);

                return InvokeCommand(message, name, instance, method);
            };
        }

        private string InvokeCommand(EventMessage message, string name, object instance, MethodInfo method)
        {
            try
            {
                _logger.LogInformation($"Command {name} has been invoked", message);
                object arguments = ConstructMethodParameters(message.JsonMessage, method);

                var result = method.Invoke(instance, new object[] { arguments });

                return JsonConvert.SerializeObject(result);
            }
            catch (TargetInvocationException ex)
            {
                _logger.LogError(ex.InnerException, "Exception was thrown for a topic", new object[] { instance.ToString(), name, method });

                throw;
            }
        }

        private void InvokeTopic(EventMessage message, object instance, KeyValuePair<string, MethodInfo> topic)
        {
            try
            {
                _logger.LogInformation($"Topic {topic.Key} has been invoked", message);
                var parameters = topic.Value.GetParameters();
                var parameter = parameters.FirstOrDefault();
                var paramType = parameter.ParameterType;
                var arguments = ConstructMethodParameters(message.JsonMessage, topic.Value);

                topic.Value.Invoke(instance, new object[] { arguments });
            }
            catch (TargetInvocationException ex)
            {
                _logger.LogError(ex.InnerException, "Exception was thrown for a topic", new object[] { instance.ToString(), topic.Key, topic.Value });
            }
        }

        private (string, MethodInfo) GetCommandMatch(string routingKey, Dictionary<string, MethodInfo> commands)
        {
            var command = commands.SingleOrDefault(c => c.Key == routingKey);

            return (command.Key, command.Value);
        }

        public Dictionary<string, MethodInfo> GetTopicMatches(string routingKey, Dictionary<string, MethodInfo> topics)
        {
            var regexHashTag = @"\w+(\.\w+)*";
            var regexStar = @"[\w]+";
            var topicMatches = new Dictionary<string, MethodInfo>();

            foreach (var topic in topics)
            {
                var pattern = topic.Key
                    .Replace(".", "\\.")
                    .Replace("*", regexStar)
                    .Replace("#", regexHashTag);

                pattern = $"^{pattern}$";

                if (Regex.IsMatch(routingKey, pattern))
                {
                    topicMatches.Add(topic.Key, topic.Value);
                }
            }

            return topicMatches;
        }

        private object ConstructMethodParameters(string message, MethodInfo method)
        {
            var parameters = method.GetParameters();
            var parameter = parameters.FirstOrDefault();

            var paramType = parameter.ParameterType;
            var arguments = JsonConvert.DeserializeObject(message, paramType);
            return arguments;
        }
    }
}
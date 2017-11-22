using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitFramework;
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
            InitializeEventListeners(types);
            _logger.LogInformation($"Initialization completed. Now listening...");
        }

        private void InitializeEventListeners(Type[] types)
        {
            _logger.LogInformation("Initializing event listeners");
            foreach (var type in types)
            {
                var eventAttribute = type.GetCustomAttribute<EventListenerAttribute>();

                if (eventAttribute != null)
                {
                    _logger.LogInformation($"Initializing event {type}");
                    Dictionary<string, MethodInfo> topicsWithMethods = GetTopicsWithMethods(type);
                    _busProvider.CreateQueueWithTopics(eventAttribute.QueueName, topicsWithMethods.Keys);
                    var callback = CreateEventReceivedCallback(type, topicsWithMethods);
                    _busProvider.BasicConsume(eventAttribute.QueueName, callback);
                }
            }
        }

        public Dictionary<string, MethodInfo> GetTopicsWithMethods(Type type)
        {
            var topicsWithMethods = new Dictionary<string, MethodInfo>();
            foreach (var methodInfo in type.GetMethods())
            {
                var topicAttribute = methodInfo.GetCustomAttribute<TopicAttribute>();

                if (topicAttribute != null)
                {
                    topicsWithMethods.Add(topicAttribute.Topic, methodInfo);
                }
            }

            return topicsWithMethods;
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

        private void InvokeTopic(EventMessage message, object instance, KeyValuePair<string, MethodInfo> topic)
        {
            try
            {
                var parameters = topic.Value.GetParameters();
                var parameter = parameters.FirstOrDefault();
                var paramType = parameter.ParameterType;
                var arguments = JsonConvert.DeserializeObject(message.JsonMessage, paramType);

                topic.Value.Invoke(instance, new object[] { arguments });
            }
            catch (TargetInvocationException ex)
            {
                _logger.LogError(ex.InnerException, "Exception was thrown for a topic", new object[] { instance.ToString(), topic.Key, topic.Value });
            }
        }
    }
}
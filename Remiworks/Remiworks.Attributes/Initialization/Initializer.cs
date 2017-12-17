using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Remiworks.Core;
using Remiworks.Core.Event;
using Remiworks.Core.Event.Listener;
using Remiworks.Core.Models;

namespace Remiworks.Attributes.Initialization
{
    public class Initializer
    {
        private ILogger Logger { get; } = RemiLogging.CreateLogger<Initializer>();
        private readonly IBusProvider _busProvider;
        private readonly IEventListener _eventListener;
        private readonly IServiceProvider _serviceProvider;

        public Initializer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            _busProvider = _serviceProvider.GetService<IBusProvider>();
            _eventListener = _serviceProvider.GetService<IEventListener>();

            if (_busProvider == null || _eventListener == null)
            {
                throw new InvalidOperationException("You did not use the AddRabbitMq method on the service collection");
            }
        }

        public void Initialize(Assembly executingAssembly)
        {
            Logger.LogInformation("Initializing event bus");
            _busProvider.CreateConnection();

            var types = executingAssembly.GetTypes();
            InitializeQueueListeners(types);
            Logger.LogInformation($"Initialization completed. Now listening...");
        }

        private void InitializeQueueListeners(IEnumerable<Type> types)
        {
            Logger.LogInformation("Initializing event listeners");
            foreach (var type in types)
            {
                var queueAttribute = type.GetCustomAttribute<QueueListenerAttribute>();

                if (queueAttribute == null)
                {
                    continue;
                }

                if (TypeContainsBothCommandsAndEvents(type))
                {
                    throw new InvalidOperationException(
                        $"Type '{type}' can't contain both events and commands. Events and commands should not be sent to the same queue.");
                }

                var methods = type.GetMethods();

                if (methods.Any(m => m.GetCustomAttributes<EventAttribute>().Any()))
                {
                    SetUpTopicMethods(type, queueAttribute.QueueName);
                    Logger.LogInformation($"Initializing event {type}");
                }
                else if (methods.Any(m => m.GetCustomAttributes<CommandAttribute>().Any()))
                {
                    SetUpCommandMethods(type, queueAttribute.QueueName);
                    Logger.LogInformation($"Initializing commands {type}");
                }
            }
        }

        private void SetUpTopicMethods(Type type, string queueName)
        {
            foreach (var topicWithMethod in GetTopicsWithMethods(type))
            {
                var parameterType = topicWithMethod.Value
                    .GetParameters()
                    .FirstOrDefault()
                    ?.ParameterType;

                if (parameterType == null)
                {
                    var exception = new InvalidOperationException(
                        $"No parameters could be found for method '{topicWithMethod.Value.Name}' in type '{type}'");
                    
                    Logger.LogError(
                        exception, 
                        "No parameters could be found for method {0} in type {1}", 
                        topicWithMethod.Value.Name, 
                        type);

                    throw exception;
                }

                _eventListener.SetupQueueListenerAsync(
                    queueName,
                    topicWithMethod.Key,
                    parameterObject => InvokeTopic(type, topicWithMethod.Key, topicWithMethod.Value, parameterObject),
                    parameterType);
            }
        }

        private void InvokeTopic(Type classType, string topic, MethodBase topicMethod, object methodParameter)
        {
            Logger.LogInformation("Invoking event topic", topic);
                        
            var instance = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, classType);

            try
            {
                topicMethod.Invoke(instance, new object[] {methodParameter});
            }
            catch (TargetInvocationException ex)
            {
                Logger.LogError(
                    ex.InnerException,
                    "Exception was thrown for a topic", 
                    instance, 
                    topic, 
                    topicMethod);

                throw;
            }
        }

        private static Dictionary<string, MethodInfo> GetTopicsWithMethods(Type type)
        {
            return GetAttributeValuesWithMethod<EventAttribute>(type, (a) => a.Topic);
        }

        #region command

        private void SetUpCommandMethods(Type type, string queueName)
        {
            Dictionary<string, MethodInfo> commandsWithMethods = GetCommandsWithMethods(type);
//            _busProvider.SetupRpcListeners(queueName, commandsWithMethods.Keys.ToArray(),
//                CreateCommandReceivedCallback(type, commandsWithMethods));
        }

        public Dictionary<string, MethodInfo> GetCommandsWithMethods(Type type)
        {
            return GetAttributeValuesWithMethod<CommandAttribute>(type, (a) => a.Key);
        }

        public CommandReceivedCallbackS CreateCommandReceivedCallback(Type type, Dictionary<string, MethodInfo> commands)
        {
            return async (message) =>
            {
                var instance = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, type);

                (string name, MethodInfo method) = GetCommandMatch(message.RoutingKey, commands);

                var result = await InvokeCommand(message, name, instance, method);

                return result;
            };
        }

        private async Task<string> InvokeCommand(EventMessage message, string name, object instance, MethodInfo method)
        {
            try
            {
                Logger.LogInformation($"Command {name} has been invoked", message);
                object[] parameters = ConstructMethodParameters(message.JsonMessage, method);

                object result = null;

                if (method.ReturnType.BaseType != typeof(Task))
                {
                    result = await Task.Run(() => method.Invoke(instance, parameters));
                }
                else
                {
                    result = await (dynamic) method.Invoke(instance, parameters);
                }

                return JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    ex.InnerException, 
                    "Exception in the class {0} was thrown for the command {1} in method {2}",
                    instance, 
                    name, 
                    method);

                throw;
            }
        }

        private static object[] ConstructMethodParameters(string message, MethodBase method)
        {
            var parameterType = method
                .GetParameters()
                .FirstOrDefault()
                ?.ParameterType;

            return parameterType != null 
                ? new[] {JsonConvert.DeserializeObject(message, parameterType)}
                : throw new InvalidOperationException($"No parameters could be found for method '{method.Name}' in type '{method.DeclaringType}'");
        }

        private static (string, MethodInfo) GetCommandMatch(string routingKey, Dictionary<string, MethodInfo> commands)
        {
            var command = commands.SingleOrDefault(c => c.Key == routingKey);

            return (command.Key, command.Value);
        }

        #endregion

        private static Dictionary<string, MethodInfo> GetAttributeValuesWithMethod<TAttribute>(
            Type type,
            Func<TAttribute, string> predicate) where TAttribute : Attribute
        {
            return type.GetMethods()
                .Where(m => m.GetCustomAttribute<TAttribute>() != null)
                .ToDictionary(m => predicate(m.GetCustomAttribute<TAttribute>()), m => m);
        }

        private static bool TypeContainsBothCommandsAndEvents(Type type)
        {
            var methods = type.GetMethods();

            return
                methods.Any(m => m.GetCustomAttributes<EventAttribute>().Any()) &&
                methods.Any(m => m.GetCustomAttributes<CommandAttribute>().Any());
        }
    }
}
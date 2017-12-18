using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remiworks.Core;
using Remiworks.Core.Event.Listener;

namespace Remiworks.Attributes.Initialization
{
    public class EventListenersInitializer
    {
        private ILogger Logger { get; } = RemiLogging.CreateLogger<Initializer>();

        private readonly IServiceProvider _serviceProvider;
        private readonly IEventListener _eventListener;

        public EventListenersInitializer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            _eventListener =
                _serviceProvider.GetService<IEventListener>() ??
                throw new InvalidOperationException("You did not use the AddRabbitMq method on the service collection");
        }

        public void InitializeEventListeners(Type type, string queueName)
        {
            Logger.LogInformation("Initializing event listeners for type '{0}'", type);

            foreach (var topicWithMethod in GetTopicsWithMethods(type))
            {
                var parameterType = InitializationUtility.GetParameterTypeOrThrow(topicWithMethod.Value, Logger);

                _eventListener.SetupQueueListenerAsync(
                    queueName,
                    topicWithMethod.Key,
                    parameterObject => InvokeTopic(type, topicWithMethod.Key, topicWithMethod.Value, parameterObject),
                    parameterType);
            }
        }

        private static Dictionary<string, MethodInfo> GetTopicsWithMethods(Type type)
        {
            return InitializationUtility.GetAttributeValuesWithMethod<EventAttribute>(type, (a) => a.Topic);
        }

        private void InvokeTopic(Type classType, string topic, MethodBase topicMethod, object methodParameter)
        {
            Logger.LogInformation("Invoking event '{0}' in type '{1}'", topic, topicMethod.DeclaringType);

            var instance = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, classType);

            try
            {
                topicMethod.Invoke(instance, new object[] { methodParameter });
            }
            catch (TargetInvocationException ex)
            {
                Logger.LogError(ex.InnerException, "Exception was thrown for event '{}'", instance, topic, topicMethod);

                throw;
            }
        }
    }
}
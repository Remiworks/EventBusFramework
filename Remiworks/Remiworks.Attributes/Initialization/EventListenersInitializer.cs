using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remiworks.Attributes.Models;
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
            Logger.LogInformation("Initializing event listeners for type '{1}'", type);

            foreach (var attributeContent in InitializationUtility.GetAttributeValuesWithMethod<EventAttribute>(type))
            {
                var parameterType = InitializationUtility.GetParameterTypeOrThrow(attributeContent.Method, Logger);
                
                Logger.LogInformation(
                    "Initializing listener for topic {1} in type {2}",
                    attributeContent.Key,
                    type);

                _eventListener.SetupQueueListenerAsync(
                    queueName,
                    attributeContent.Key,
                    (receivedParameter, receivedTopic) => InvokeTopic(receivedParameter, receivedTopic, attributeContent),
                    parameterType);
            }
        }

        private void InvokeTopic(object receivedParameter, string receivedTopic, AttributeContent attributeContent)
        {
            var declaringType = attributeContent.Method.DeclaringType;
            
            Logger.LogInformation(
                "Invoking event '{1}' in type '{2}'", 
                attributeContent.Key, 
                declaringType.FullName);

            try
            {
                var instance = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, declaringType);
                attributeContent.Method.Invoke(instance, new[] { receivedParameter});
            }
            catch (TargetInvocationException ex)
            {
                Logger.LogError(
                    ex.InnerException, 
                    "Exception was thrown for event '{1}' in type {2}",
                    attributeContent.Key,
                    declaringType.FullName);

                throw;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Remiworks.Core;

namespace Remiworks.Attributes.Initialization
{
    public class Initializer
    {
        private ILogger Logger { get; } = RemiLogging.CreateLogger<Initializer>();

        private readonly EventListenersInitializer _eventListenersInitializer;
        private readonly CommandListenersInitializer _commandListenersInitializer;

        private readonly IServiceProvider _serviceProvider;

        public Initializer(IServiceProvider serviceProvider)
        {
            _eventListenersInitializer = new EventListenersInitializer(serviceProvider);
            _commandListenersInitializer = new CommandListenersInitializer(serviceProvider);

            _serviceProvider = serviceProvider;
        }

        public void Initialize(Assembly executingAssembly)
        {
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
                    _eventListenersInitializer.InitializeEventListeners(type, queueAttribute.QueueName);
                }
                else if (methods.Any(m => m.GetCustomAttributes<CommandAttribute>().Any()))
                {
                    _commandListenersInitializer.InitializeCommandListeners(type, queueAttribute.QueueName);
                }
            }
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
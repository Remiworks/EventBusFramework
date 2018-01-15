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
            CheckDependencies(types);

            InitializeQueueListeners(types);
            Logger.LogInformation($"Initialization completed. Now listening...");
        }

        private void CheckDependencies(Type[] types)
        {
            Logger.LogInformation($"Checking dependencies...");

            var errors = DependencyInjectionChecker.CheckDependencies(_serviceProvider, types.ToList()).ToList();

            if (errors.Any())
            {
                LogDependencyErrors(errors);

                throw new AggregateException(
                    errors.Select(error => new DependencyInjectionException(error)));
            }
        }

        private void LogDependencyErrors(IEnumerable<string> errors)
        {
            foreach(var error in errors)
            {
                Logger.LogCritical(error);
            }
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
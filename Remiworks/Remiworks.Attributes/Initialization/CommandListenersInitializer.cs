using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remiworks.Core;
using Remiworks.Core.Command.Listener;

namespace Remiworks.Attributes.Initialization
{
    public class CommandListenersInitializer
    {
        private ILogger Logger { get; } = RemiLogging.CreateLogger<Initializer>();

        private readonly IServiceProvider _serviceProvider;
        private readonly ICommandListener _commandListener;

        public CommandListenersInitializer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            _commandListener =
                _serviceProvider.GetService<ICommandListener>() ??
                throw new InvalidOperationException("You did not use the AddRabbitMq method on the service collection");
        }

        public void InitializeCommandListeners(Type type, string queueName)
        {
            Logger.LogInformation("Initializing command listeners for type '{0}'", type);

            foreach (var keyWithMethod in GetCommandsWithMethods(type))
            {
                var parameterType = InitializationUtility.GetParameterTypeOrThrow(keyWithMethod.Value, Logger);

                _commandListener.SetupCommandListenerAsync(
                    queueName,
                    keyWithMethod.Key,
                    parameterObject => InvokeCommand(keyWithMethod.Key, keyWithMethod.Value, parameterObject),
                    parameterType).Wait();
            }
        }

        private static Dictionary<string, MethodInfo> GetCommandsWithMethods(Type type)
        {
            return InitializationUtility.GetAttributeValuesWithMethod<CommandAttribute>(type, (a) => a.Key);
        }

        private object InvokeCommand(string topic, MethodInfo topicMethod, object methodParameter)
        {
            Logger.LogInformation("Invoking command '{0}'", topic);

            var instance = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, topicMethod.DeclaringType);

            try
            {
                var result = topicMethod.Invoke(instance, new object[] { methodParameter });

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.InnerException, "Exception in the class {0} was thrown for the command {1} in method {2}", instance, topic, topicMethod);

                throw;
            }
        }
    }
}
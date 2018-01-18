using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remiworks.Attributes.Models;
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
            Logger.LogInformation("Initializing command listeners for type '{1}'", type);

            foreach (var attributeContent in InitializationUtility.GetAttributeValuesWithMethod<CommandAttribute>(type))
            {
                var parameterType = InitializationUtility.GetParameterTypeOrThrow(attributeContent.Method, Logger);

                _commandListener.SetupCommandListenerAsync(
                    queueName,
                    attributeContent.Key,
                    receivedParameter => InvokeCommand(receivedParameter, attributeContent),
                    parameterType,
                    attributeContent.ExchangeName).Wait();
            }
        }

        private async Task<object> InvokeCommand(object receivedParameter, AttributeContent attributeContent)
        {
            var declaringType = attributeContent.Method.DeclaringType;
            
            Logger.LogInformation(
                "Invoking command '{1}' in type '{2}'", 
                attributeContent, 
                declaringType.FullName);

            try
            {
                var instance = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, declaringType);
                
                return IsTask(attributeContent.Method)
                    ? await (dynamic)attributeContent.Method.Invoke(instance, new[] { receivedParameter})
                    : await Task.Run(() => attributeContent.Method.Invoke(instance, new[] { receivedParameter }));
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception was thrown for command '{1}' in type {2}. Exception message: {3}. InnerException: {4}", 
                    attributeContent.Key,
                    declaringType.FullName, 
                    ex.Message, 
                    ex.InnerException);
         

                throw;
            }
        }

        private static bool IsTask(MethodInfo method)
        {
            return method.ReturnType.BaseType == typeof(Task);
        }
    }
}
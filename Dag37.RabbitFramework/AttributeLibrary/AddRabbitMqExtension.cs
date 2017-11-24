using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitFramework;
using RabbitFramework.Contracts;
using RabbitFramework.Models;
using System;
using System.Reflection;

namespace AttributeLibrary
{
    public static class AddRabbitMqExtension
    {
        public static IServiceCollection AddRabbitMq(this IServiceCollection serviceCollection, BusOptions options, ILoggerFactory logger = null)
        {
            if (logger != null)
            {
                RabbitLogging.LoggerFactory = logger;
            }

            return serviceCollection.AddTransient<IBusProvider>((ctx) =>
            {
                return new RabbitBusProvider(options);
            });
        }

        public static void UseRabbitMq(this IServiceProvider serviceProvider)
        {
            var busProvider = serviceProvider.GetService<IBusProvider>();

            if (busProvider == null)
            {
                throw new NotImplementedException("You did not use the AddRabbitMq method on the service collection");
            }

            var initializer = new RabbitInitializer(serviceProvider.GetService<IBusProvider>(), serviceProvider);

            initializer.Initialize(Assembly.GetCallingAssembly());
        }
    }
}
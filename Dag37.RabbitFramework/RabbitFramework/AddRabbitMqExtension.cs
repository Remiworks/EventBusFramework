using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RabbitFramework
{
    public static class AddRabbitMqExtension
    {
        public static IServiceCollection AddRabbitMq(this IServiceCollection serviceCollection, BusOptions options)
        {
            return serviceCollection.AddTransient<IBusProvider>((ctx) =>
            {
                return new RabbitBusProvider(options);
            });
        }

        public static void UseRabbitMq(this IServiceProvider serviceProvider)
        {
            string Host = "localhost";
            int Port = 5672;
            string UserName = "guest";
            string Password = "guest";
            string ExchangeName = "testExchange";

            var initializer = new RabbitInitializer(serviceProvider.GetService<IBusProvider>(), serviceProvider);

            initializer.Initialize(Assembly.GetCallingAssembly());
        }
    }
}

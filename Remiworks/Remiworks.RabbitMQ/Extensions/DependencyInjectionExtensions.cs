using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remiworks.Core;
using Remiworks.Core.Command;
using Remiworks.Core.Models;

namespace Remiworks.RabbitMQ.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddRabbitMq(this IServiceCollection serviceCollection, BusOptions options, ILoggerFactory logger = null)
        {
            if (logger != null)
            {
                RemiLogging.LoggerFactory = logger;
            }

            return serviceCollection
                .AddSingleton<IBusProvider>(ctx => new RabbitBusProvider(options))
                .AddSingleton<ICommandPublisher, CommandPublisher>();
        }
    }
}
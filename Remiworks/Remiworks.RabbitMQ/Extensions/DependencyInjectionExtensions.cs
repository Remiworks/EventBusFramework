using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remiworks.Core;
using Remiworks.Core.Command.Listener;
using Remiworks.Core.Command.Listener.Callbacks;
using Remiworks.Core.Command.Publisher;
using Remiworks.Core.Event.Listener;
using Remiworks.Core.Event.Listener.Callbacks;
using Remiworks.Core.Event.Publisher;
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
                .AddSingleton<ICommandPublisher, CommandPublisher>()
                .AddSingleton<ICommandListener, CommandListener>()
                .AddSingleton<ICommandCallbackRegistry, CommandCallbackRegistry>()
                .AddSingleton<IEventPublisher, EventPublisher>()
                .AddSingleton<IEventListener, EventListener>()
                .AddSingleton<IEventCallbackRegistry, EventCallbackRegistry>();
        }
    }
}
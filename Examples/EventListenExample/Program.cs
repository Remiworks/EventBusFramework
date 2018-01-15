using EventListenExample.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remiworks.Core.Models;
using Remiworks.RabbitMQ.Extensions;
using Remiworks.Attributes.Extensions;
using Serilog;

namespace EventListenExample
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var loggerFactory = new LoggerFactory()
                .AddSerilog(logger);
            
            var serviceProvider = new ServiceCollection()
                .AddTransient<OrderPickingController>()
                .AddRabbitMq(new BusOptions(), loggerFactory)
                .BuildServiceProvider();

            serviceProvider.UseAttributes();
        }
    }
}
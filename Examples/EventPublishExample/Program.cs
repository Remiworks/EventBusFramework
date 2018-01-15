using System.Collections.Generic;
using EventPublishExample.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remiworks.Core.Models;
using Remiworks.RabbitMQ.Extensions;
using Serilog;

namespace EventPublishExample
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
                .AddTransient<OrderController>()
                .AddRabbitMq(new BusOptions(), loggerFactory)
                .BuildServiceProvider();

            var controller = serviceProvider.GetService<OrderController>();

            controller
                .PlaceOrder("Landstraat 3", 126.55M, new List<string> { "iPhone charger", "Pickels", "Banana" })
                .Wait();
        }
    }
}
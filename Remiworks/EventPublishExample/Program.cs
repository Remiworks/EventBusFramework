using EventPublishExample.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Remiworks.Core.Models;
using Remiworks.RabbitMQ.Extensions;

namespace EventPublishExample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // This can also be done with an MVC application.
            // Just hook into the Startup.cs there
            var serviceProvider = new ServiceCollection()
                .AddTransient<OrderPickingController>()
                .AddRabbitMq(new BusOptions())
                .BuildServiceProvider();
        }
    }
}
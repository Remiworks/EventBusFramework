using EventListenExample.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Remiworks.Core.Models;
using Remiworks.RabbitMQ.Extensions;
using Remiworks.Attributes.Extensions;

namespace EventListenExample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddTransient<OrderPickingController>()
                .AddRabbitMq(new BusOptions())
                .BuildServiceProvider();

            serviceProvider.UseAttributes();
        }
    }
}
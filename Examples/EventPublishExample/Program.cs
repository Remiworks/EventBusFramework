using System;
using System.Collections.Generic;
using EventPublishExample.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Remiworks.Core.Models;
using Remiworks.RabbitMQ.Extensions;

namespace EventPublishExample
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Instantiating dependency injection boilerplate");

            // This can also be done with an MVC application.
            // Just hook into the Startup.cs there
            var serviceProvider = new ServiceCollection()
                .AddTransient<OrderController>()
                .AddRabbitMq(new BusOptions())
                .BuildServiceProvider();

            var controller = serviceProvider.GetService<OrderController>();

            Console.WriteLine("Calling controller to place order");

            // Place order and await the call
            controller
                .PlaceOrder("Landstraat 3", 126.55M, new List<string> { "iPhone charger", "Pickels", "Banana" })
                .Wait();
        }
    }
}
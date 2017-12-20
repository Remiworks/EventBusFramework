using System;
using Microsoft.Extensions.DependencyInjection;
using Remiworks.Core.Models;
using Remiworks.RabbitMQ.Extensions;
using RpcTest.Controllers;
using Remiworks.Attributes.Extensions;

namespace RpcTest
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            // Register services and add RabbitMQ to the mix

            var serviceProvider = new ServiceCollection()
                .AddTransient<SomeController>()
                .AddRabbitMq(new BusOptions())
                .BuildServiceProvider();

            serviceProvider.UseAttributes();
            
            // Get a reference to SomeController and call SendExampleCommand(...)
            var someController = serviceProvider.GetService<SomeController>();
            int amount = 10;

            Console.WriteLine($"Requesting fib({amount})");

            var fib = someController.SendExampleCommand(amount).Result;

            Console.WriteLine($"Got '{fib}'");
        }
    }
}
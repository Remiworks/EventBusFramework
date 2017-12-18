using System;
using Microsoft.Extensions.DependencyInjection;
using Remiworks.Core.Models;
using Remiworks.RabbitMQ.Extensions;
using RpcTest.Controllers;

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

            // Get a reference to SomeController and call SendExampleCommand(...)
            var someController = serviceProvider.GetService<SomeController>();
            int amount = 10;

            Console.WriteLine($"Requesting fib({amount})");

            var fib = someController.SendExampleCommand(amount).Result;

            Console.WriteLine($"Got '{fib}'");
        }
    }
}
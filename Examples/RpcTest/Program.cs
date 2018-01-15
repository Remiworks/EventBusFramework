using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remiworks.Core.Models;
using Remiworks.RabbitMQ.Extensions;
using RpcTest.Controllers;
using Serilog;

namespace RpcTest
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
                .AddTransient<SomeController>()
                .AddRabbitMq(new BusOptions(), loggerFactory)
                .BuildServiceProvider();
            
            var someController = serviceProvider.GetService<SomeController>();
            const int amount = 10;

            // This illustrates a remote method which returns a result
            Console.WriteLine($"Requesting fib({amount})");
            var fib = someController.SendExampleCommandWithResult(amount).Result;
            Console.WriteLine($"Got '{fib}'\n");

            // This illustrates a remote method which returns nothing (void)
            Console.WriteLine($"Sending command to listener with void return type");
            someController.SendExampleCommandWithoutResult(amount).Wait();
            Console.WriteLine($"Void finished executing");

            Console.ReadKey();
        }
    }
}
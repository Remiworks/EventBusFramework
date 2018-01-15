using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remiworks.Core.Models;
using Remiworks.RabbitMQ.Extensions;
using Remiworks.Attributes.Extensions;
using RpcServerTest.Controllers;
using Serilog;

namespace RpcServerTest
{
    public static class Program
    {
        public static void Main()
        {
            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var loggerFactory = new LoggerFactory()
                .AddSerilog(logger);
            
            
            var serviceProvider = new ServiceCollection()
                .AddTransient<FibController>()
                .AddRabbitMq(new BusOptions(), loggerFactory)
                .BuildServiceProvider();

            serviceProvider.UseAttributes();
            
            Console.ReadLine();
        }
    }
}
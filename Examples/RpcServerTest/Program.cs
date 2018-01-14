using System;
using Microsoft.Extensions.DependencyInjection;
using Remiworks.Core.Models;
using Remiworks.RabbitMQ.Extensions;
using Remiworks.Attributes.Extensions;
using RpcServerTest.Controllers;

namespace RpcServerTest
{
    public static class Program
    {
        public static void Main()
        {
            var serviceProvider = new ServiceCollection()
                .AddTransient<FibController>()
                .AddRabbitMq(new BusOptions())
                .BuildServiceProvider();

            serviceProvider.UseAttributes();
            
            Console.ReadLine();
        }
    }
}
using Microsoft.Extensions.DependencyInjection;
using RabbitFramework;
using System;

namespace RabbitmqReceiver
{
    public class Program
    {
        private const string Host = "localhost";
        private const int Port = 5672;
        private const string UserName = "guest";
        private const string Password = "guest";
        private const string ExchangeName = "testExchange";
        private const string TopicType = "topic";

        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddTransient<Mather>()
                .AddRabbitMq(new BusOptions { ExchangeName = ExchangeName, Hostname = Host, Password = Password, Port = Port, UserName = UserName })
                .BuildServiceProvider();

            serviceProvider.UseRabbitMq();

            Console.ReadLine();
        }
    }
}

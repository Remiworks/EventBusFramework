using RabbitFramework;
using System;

namespace TestConsoleAppRabbi
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

            RabbitInitializer initializer = new RabbitInitializer(new RabbitBusProvider(new BusOptions
            {
                Hostname = Host,
                Port = Port,
                UserName = UserName,
                Password = Password,
                ExchangeName = ExchangeName
            }));

            initializer.Initialize();

            Console.ReadLine();
        }
    }
}

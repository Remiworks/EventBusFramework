using RabbitFramework;
using RabbitMQ.Client;
using System;

namespace RpcServerTest
{
    public static class Program
    {
        private static IConnection _connection;
        private static IModel _channel;

        public static void Main()
        {
            using (IBusProvider server = new RabbitBusProvider(new BusOptions()))
            {
                server.SetupRpcListener<SomeCommand>("rpc_queue", Fib);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        private static object Fib(SomeCommand n)
        {
            return CalculateFib(n.Value);
        }

        private static int CalculateFib(int n)
        {
            if (n == 0 || n == 1)
            {
                return n;
            }

            return CalculateFib(n - 1) + CalculateFib(n - 2);
        }
    }

    public class SomeCommand
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}
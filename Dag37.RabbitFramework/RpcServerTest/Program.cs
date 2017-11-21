using RabbitFramework;
using System;

namespace RpcServerTest
{
    public static class Program
    {
        public static void Main()
        {
            using (IBusProvider server = new RabbitBusProvider(new BusOptions()))
            {
                server.SetupRpcListener<SomeCommand>("rpc_queue", Fib);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }

    public class SomeCommand
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}
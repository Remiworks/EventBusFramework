using RabbitFramework;
using System;

namespace RpcTest
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            using (IBusProvider rpcClient = new RabbitBusProvider(new BusOptions()))
            {
                var command = new SomeCommand { Name = "Something", Value = 10 };
                Console.WriteLine($" [x] Requesting fib({command.Value})");

                string response = rpcClient.Call<string>("rpc_queue", command).Result;

                Console.WriteLine(" [.] Got '{0}'", response);
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
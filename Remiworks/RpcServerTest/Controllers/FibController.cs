using System;
using System.Threading.Tasks;
using Remiworks.Attributes;
using RpcServerTest.Commands;

namespace RpcServerTest.Controllers
{
    [QueueListener("rcpTestQueue")]
    public class FibController
    {
        [Command("calculate.fib")]
        public int HandleCalculateCommand(SomeCommand command)
        {
            Console.WriteLine($"Got fib-request for {command.Value}");

            var fibResult = CalculateFib(command.Value);
            Console.WriteLine($"Calculated fib result: {fibResult}");

            return fibResult;
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
}
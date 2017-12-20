using System;
using Remiworks.Attributes;
using Remiworks.Core.Command.Publisher;
using RpcServerTest.Commands;

namespace RpcServerTest.Controllers
{
    [QueueListener("rcpTestQueue")]
    public class FibController
    {
        private readonly ICommandPublisher _commandPublisher;

        public FibController(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        [Command("calculate.fib")]
        public int HandleCalculateCommand(SomeCommand command)
        {
            Console.WriteLine($"Got fib-request for {command.Value}");

            var fibResult = CalculateFib(command.Value);

            Console.WriteLine($"Calculated fib result: {fibResult}\n");

            return fibResult;
        }

        [Command("do.something")]
        public void HandleVoidCommand(SomeCommand command)
        {
            Console.WriteLine($"Got void-request for {command.Value}");
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
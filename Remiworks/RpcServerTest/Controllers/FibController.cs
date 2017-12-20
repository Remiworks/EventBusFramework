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
            // 2
            Console.WriteLine($"Got fib-request for {command.Value}");

            //var fibResult = CalculateFib(command.Value);

            // 3 Boem!
            var fibResult = _commandPublisher
                .SendCommandAsync<int>(command, "SomeQueue", "some.thing")
                .Result;
            
            Console.WriteLine($"Calculated fib result: {fibResult}");

            return fibResult;
        }

        //private static int CalculateFib(int n)
        //{
        //    if (n == 0 || n == 1)
        //    {
        //        return n;
        //    }

        //    return CalculateFib(n - 1) + CalculateFib(n - 2);
        //}
    }
}
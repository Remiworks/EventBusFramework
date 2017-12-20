using System;
using System.Threading.Tasks;
using Remiworks.Attributes;
using Remiworks.Core.Command.Publisher;
using RpcTest.Commands;

namespace RpcTest.Controllers
{
    [QueueListener("SomeQueue")]
    public class SomeController
    {
        private const string QueueName = "rcpTestQueue";
        private const string FibCommandKey = "calculate.fib";

        private readonly ICommandPublisher _commandPublisher;

        public SomeController(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public async Task<int> SendExampleCommand(int amount)
        {
            var command = new SomeCommand { Value = amount };
            
            // 1
            var result =  await _commandPublisher.SendCommandAsync<int>(command, QueueName, FibCommandKey, 50000);
            
            // 4
            return result;
        }

        [Command("some.thing")]
        public int DoSomethingElse(SomeCommand command)
        {
            Console.WriteLine($"Got some other request {command.Value}");

            return command.Value * 2;
        }
    }
}
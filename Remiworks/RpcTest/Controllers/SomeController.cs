using System;
using System.Threading.Tasks;
using Remiworks.Core.Command.Publisher;
using RpcTest.Commands;

namespace RpcTest.Controllers
{
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

            return await _commandPublisher.SendCommandAsync<int>(command, QueueName, FibCommandKey);
        }
    }
}
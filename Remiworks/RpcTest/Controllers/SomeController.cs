using System;
using System.Threading.Tasks;
using Remiworks.Attributes;
using Remiworks.Core.Command.Publisher;
using RpcTest.Commands;

namespace RpcTest.Controllers
{
    public class SomeController
    {
        private const string QueueName = "rcpTestQueue";
        private const string FibCommandKey = "calculate.fib";
        private const string VoidCommandKey = "do.something";

        private readonly ICommandPublisher _commandPublisher;

        public SomeController(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public async Task<int> SendExampleCommandWithResult(int amount)
        {
            var command = new SomeCommand { Value = amount };

            return await _commandPublisher.SendCommandAsync<int>(
                command, 
                QueueName, 
                FibCommandKey);
        }

        public async Task SendExampleCommandWithoutResult(int amount)
        {
            var command = new SomeCommand { Value = amount };

            await _commandPublisher.SendCommandAsync(
                command,
                QueueName,
                VoidCommandKey);
        }
    }
}
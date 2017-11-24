using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitFramework.Contracts;
using RabbitFramework.Models;

namespace RabbitFramework.Publishers
{
    public class CommandPublisher : ICommandPublisher
    {
        private readonly string _callbackQueue;
        private readonly IBusProvider _busProvider;
        private readonly ConcurrentDictionary<Guid, Action<string>> _commandCallbacks;

        public CommandPublisher(IBusProvider busProvider)
        {
            _busProvider = busProvider;
            _commandCallbacks = new ConcurrentDictionary<Guid, Action<string>>();

            _callbackQueue = $"CommandQueue-{Guid.NewGuid().ToString()}";
            // TODO: Declare Queue
            _busProvider.BasicConsume(_callbackQueue, HandleCommandCallback);
        }

        public async Task<TResult> SendCommand<TResult>(object message, string queueName, string key, int timeout = 5000)
        {
            if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentNullException(nameof(queueName));
            else if (message == null) throw new ArgumentNullException(nameof(message));
            else if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            var correlationId = Guid.NewGuid();

            var waitHandle = new ManualResetEvent(false);
            string responseJson = null;

            _commandCallbacks[correlationId] = (response) =>
            {
                responseJson = response;
                waitHandle.Set();
            };

            PublishCommandMessage(correlationId, key, message);

            bool gotResponse = await Task.Run(() => waitHandle.WaitOne(timeout));

            return gotResponse
                ? JsonConvert.DeserializeObject<TResult>(responseJson)
                : throw new TimeoutException($"Could not get response for the command '{correlationId}' in queue '{queueName}'");
        }

        private void PublishCommandMessage(Guid correlationId, string key, object message)
        {
            _busProvider.BasicPublish(new EventMessage
            {
                CorrelationId = correlationId,
                RoutingKey = key,
                JsonMessage = JsonConvert.SerializeObject(message),
                ReplyQueueName = _callbackQueue,
            });
        }

        private void HandleCommandCallback(EventMessage message)
        {
            if (message.CorrelationId.HasValue && _commandCallbacks.ContainsKey(message.CorrelationId.Value))
            {
                _commandCallbacks[message.CorrelationId.Value](message.JsonMessage);
            }
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Newtonsoft.Json;
using Remiworks.Core.Exceptions;
using Remiworks.Core.Models;

namespace Remiworks.Core.Command.Publisher
{
    public class CommandPublisher : ICommandPublisher
    {
        private readonly string _callbackQueue;
        private readonly IBusProvider _busProvider;
        private readonly ConcurrentDictionary<Guid, Action<string, bool>> _commandCallbacks;

        public CommandPublisher(IBusProvider busProvider)
        {
            EnsureArg.IsNotNull(busProvider, nameof(busProvider));
            
            _busProvider = busProvider;
            _commandCallbacks = new ConcurrentDictionary<Guid, Action<string, bool>>();

            _callbackQueue = $"CommandQueue-{Guid.NewGuid().ToString()}";
            _busProvider.BasicConsume(_callbackQueue, HandleCommandCallback);
        }

        public async Task<T> SendCommandAsync<T>(object message, string queueName, string key, int timeout = 5000)
        {
            EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));
            EnsureArg.IsNotNull(message, nameof(message));
            EnsureArg.IsNotNullOrWhiteSpace(key, nameof(key));
            
            if (key.Contains("*") || key.Contains("#")) throw new ArgumentException("Key shouldn't contain wildcards");

            var correlationId = Guid.NewGuid();

            var waitHandle = new ManualResetEvent(false);
            string responseJson = null;
            var isError = false;

            _commandCallbacks[correlationId] = (response, responseIsError) =>
            {
                responseJson = response;
                isError = responseIsError;
                waitHandle.Set();
            };

            PublishCommandMessage(message, queueName, key, correlationId);

            var gotResponse = await Task.Run(() => waitHandle.WaitOne(timeout));

            if (isError)
            {
                var exception = JsonConvert.DeserializeObject<CommandPublisherException>(responseJson);
                
                throw exception;
            }

            return gotResponse
                ? JsonConvert.DeserializeObject<T>(responseJson)
                : throw new TimeoutException($"Could not get response for the command '{correlationId}' in queue '{queueName}'");
        }

        private void PublishCommandMessage(object message, string queueName, string key, Guid correlationId)
        {
            _busProvider.CreateTopicsForQueue(queueName, key);

            _busProvider.BasicPublish(new EventMessage
            {
                CorrelationId = correlationId,
                RoutingKey = key,
                JsonMessage = JsonConvert.SerializeObject(message),
                ReplyQueueName = _callbackQueue
            });
        }

        private void HandleCommandCallback(EventMessage message)
        {
            if (message.CorrelationId.HasValue && _commandCallbacks.ContainsKey(message.CorrelationId.Value))
            {
                _commandCallbacks[message.CorrelationId.Value](message.JsonMessage, message.IsError);
            }
        }
    }
}
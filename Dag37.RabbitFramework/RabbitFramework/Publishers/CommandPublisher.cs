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
        private readonly ConcurrentDictionary<Guid, Action<string, bool>> _commandCallbacks;

        public CommandPublisher(IBusProvider busProvider)
        {
            _busProvider = busProvider;
            _commandCallbacks = new ConcurrentDictionary<Guid, Action<string, bool>>();

            _callbackQueue = $"CommandQueue-{Guid.NewGuid().ToString()}";
            _busProvider.BasicConsume(_callbackQueue, HandleCommandCallback);
        }

        public async Task<T> SendCommandAsync<T>(object message, string queueName, string key, int timeout = 5000)
        {
            if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentNullException(nameof(queueName));
            else if (message == null) throw new ArgumentNullException(nameof(message));
            else if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            else if (key.Contains("*") || key.Contains("#")) throw new ArgumentException("Key may not contain wildcards");

            var correlationId = Guid.NewGuid();

            var waitHandle = new ManualResetEvent(false);
            string responseJson = null;
            bool isError = false;

            _commandCallbacks[correlationId] = (response, responseIsError) =>
            {
                responseJson = response;
                isError = responseIsError;
                waitHandle.Set();
            };

            PublishCommandMessage(message, queueName, key, correlationId);

            bool gotResponse = await Task.Run(() => waitHandle.WaitOne(timeout));

            if (isError)
            {
                throw new CommandPublisherException(responseJson);
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
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
        private const string TimeoutExceptionMessage = "Could not get response for the command '{0}' in queue '{1}'";
        private const string NoWildcardExceptionMessage = "Key shouldn't contain wildcards";

        private readonly string _callbackQueue;
        private readonly IBusProvider _busProvider;
        private readonly ConcurrentDictionary<Guid, Action<string, bool>> _commandCallbacks;

        public CommandPublisher(IBusProvider busProvider)
        {
            EnsureArg.IsNotNull(busProvider, nameof(busProvider));

            _busProvider = busProvider;
            _commandCallbacks = new ConcurrentDictionary<Guid, Action<string, bool>>();

            _busProvider.EnsureConnection();

            _callbackQueue = $"CommandQueue-{Guid.NewGuid().ToString()}";
            _busProvider.BasicConsume(_callbackQueue, HandleCommandCallback);
        }

        public async Task<T> SendCommandAsync<T>(object message, string queueName, string key, int timeout = 5000)
        {
            EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));
            EnsureArg.IsNotNull(message, nameof(message));
            EnsureArg.IsNotNullOrWhiteSpace(key, nameof(key));

            (var gotResponse, var responseJson) = await SendAndListenToCommandAsync(message, queueName, key, timeout);

            return gotResponse
                ? JsonConvert.DeserializeObject<T>(responseJson)
                : throw new TimeoutException(string.Format(TimeoutExceptionMessage, key, queueName));
        }

        public async Task<T> SendCommandAsync<T>(object message, string queueName, string key, string exchangeName,
            int timeout = 5000)
        {
            EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));
            EnsureArg.IsNotNull(message, nameof(message));
            EnsureArg.IsNotNullOrWhiteSpace(key, nameof(key));
            EnsureArg.IsNotNull(exchangeName, nameof(exchangeName));

            (var gotResponse, var responseJson) =
                await SendAndListenToCommandAsync(message, queueName, key, timeout, exchangeName);

            return gotResponse
                ? JsonConvert.DeserializeObject<T>(responseJson)
                : throw new TimeoutException(string.Format(TimeoutExceptionMessage, key, queueName));
        }

        public async Task SendCommandAsync(object message, string queueName, string key, int timeout = 5000)
        {
            EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));
            EnsureArg.IsNotNull(message, nameof(message));
            EnsureArg.IsNotNullOrWhiteSpace(key, nameof(key));

            (var gotResponse, var responseJson) = await SendAndListenToCommandAsync(message, queueName, key, timeout);

            if (!gotResponse)
            {
                throw new TimeoutException(string.Format(TimeoutExceptionMessage, key, queueName));
            }
        }

        public async Task SendCommandAsync(object message, string queueName, string key, string exchangeName, int timeout = 5000)
        {
            EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));
            EnsureArg.IsNotNull(message, nameof(message));
            EnsureArg.IsNotNullOrWhiteSpace(key, nameof(key));
            EnsureArg.IsNotNull(exchangeName, nameof(exchangeName));

            (var gotResponse, var responseJson) =
                await SendAndListenToCommandAsync(message, queueName, key, timeout, exchangeName);

            if (!gotResponse)
            {
                throw new TimeoutException(string.Format(TimeoutExceptionMessage, key, queueName));
            }
        }

        private async Task<(bool gotResponse, string responseJson)> SendAndListenToCommandAsync(
            object message,
            string queueName,
            string key,
            int timeout,
            string exchangeName = null)
        {
            if (key.Contains("*") || key.Contains("#")) throw new ArgumentException(NoWildcardExceptionMessage);

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

            PublishCommandMessage(message, queueName, key, correlationId, exchangeName);

            var gotResponse = await Task.Run(() => waitHandle.WaitOne(timeout));

            if (isError)
            {
                var exception = JsonConvert.DeserializeObject<CommandPublisherException>(responseJson);

                throw exception;
            }

            return (gotResponse, responseJson);
        }

        private void PublishCommandMessage(object message, string queueName, string key, Guid correlationId, string exchangeName)
        {
            var eventMessage = new EventMessage
            {
                CorrelationId = correlationId,
                RoutingKey = key,
                Type = key,
                JsonMessage = JsonConvert.SerializeObject(message),
                ReplyQueueName = _callbackQueue
            };

            switch (exchangeName)
            {
                case null:
                    _busProvider.BasicTopicBind(queueName, key);
                    _busProvider.BasicPublish(eventMessage);
                    break;
                case "": // "" is actually a exchange of type 'direct' in rabbitMQ. No topicbind needed in this case
                    _busProvider.BasicPublish(eventMessage, exchangeName);
                    break;
                default:
                    _busProvider.BasicTopicBind(queueName, key, exchangeName);
                    _busProvider.BasicPublish(eventMessage, exchangeName);
                    break;
            }
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
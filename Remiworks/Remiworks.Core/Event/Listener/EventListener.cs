using System;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Remiworks.Core.Event.Listener.Callbacks;
using Remiworks.Core.Models;

namespace Remiworks.Core.Event.Listener
{
    public class EventListener : IEventListener
    {
        private ILogger Logger { get; } = RemiLogging.CreateLogger<EventListener>();

        private readonly IBusProvider _busProvider;
        private readonly IEventCallbackRegistry _callbackRegistry;

        public EventListener(IBusProvider busProvider, IEventCallbackRegistry callbackRegistry)
        {
            _busProvider = busProvider;
            _callbackRegistry = callbackRegistry;

            _busProvider.EnsureConnection();
        }

        public Task SetupQueueListenerAsync<TParam>(
            string queueName,
            string topic,
            EventReceived<TParam> callback,
            string exchangeName = null)
        {
            EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));
            EnsureArg.IsNotNullOrWhiteSpace(topic, nameof(topic));
            EnsureArg.IsNotNull(callback, nameof(callback));

            return SetupQueueListenerAsync(
                queueName,
                topic,
                (input, receivedTopic) => callback((TParam)input, receivedTopic),
                typeof(TParam),
                exchangeName);
        }

        public Task SetupQueueListenerAsync(
            string queueName, 
            string topic,
            EventReceived callback, 
            Type parameterType,
            string exchangeName = null)
        {
            EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));
            EnsureArg.IsNotNullOrWhiteSpace(topic, nameof(topic));
            EnsureArg.IsNotNull(callback, nameof(callback));
            EnsureArg.IsNotNull(parameterType, nameof(parameterType));

            LogSetupQueueListenerCalled(queueName, topic, parameterType, exchangeName);

            return Task.Run(() =>
            {
                void CallbackInvoker(EventMessage eventMessage)
                {
                    var deserializedParamter = JsonConvert.DeserializeObject(eventMessage.JsonMessage, parameterType);

                    callback(deserializedParamter, eventMessage.RoutingKey);
                }

                _callbackRegistry.AddCallbackForQueue(queueName, topic, CallbackInvoker, exchangeName);
            });
        }

        private void LogSetupQueueListenerCalled(string queueName, string topic, Type parameterType, string exchangeName)
        {
            if (exchangeName == null)
            {
                Logger.LogInformation(
                    "Initializing queue listener for queue {0}, topic {1} and parameter type {2}",
                    queueName,
                    topic,
                    parameterType.FullName);
            }
            else
            {
                Logger.LogInformation(
                    "Initializing queue listener for queue {0}, topic {1}, parameter type {2} and exchange {3}",
                    queueName,
                    topic,
                    parameterType.FullName,
                    exchangeName);
            }
        }
    }
}
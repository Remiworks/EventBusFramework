using System;
using System.Threading.Tasks;
using EnsureThat;
using Newtonsoft.Json;
using Remiworks.Core.Event.Listener.Callbacks;
using Remiworks.Core.Models;

namespace Remiworks.Core.Event.Listener
{
    public class EventListener : IEventListener
    {
        private readonly IBusProvider _busProvider;
        private readonly IEventCallbackRegistry _callbackRegistry;

        public EventListener(IBusProvider busProvider, IEventCallbackRegistry callbackRegistry)
        {
            _busProvider = busProvider;
            _callbackRegistry = callbackRegistry;
        }

        public Task SetupQueueListenerAsync<TParam>(string queueName, EventReceived<TParam> callback)
        {
            EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));
            EnsureArg.IsNotNull(callback, nameof(callback));

            return SetupQueueListenerAsync(
                queueName,
                (input, topic) => callback((TParam)input, topic),
                typeof(TParam));
        }

        public Task SetupQueueListenerAsync(string queueName, EventReceived callback, Type parameterType)
        {
            EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));
            EnsureArg.IsNotNull(callback, nameof(callback));
            EnsureArg.IsNotNull(parameterType, nameof(parameterType));

            return Task.Run(() =>
            {
                void ReceivedCallback(EventMessage eventMessage)
                {
                    var messageObject = JsonConvert.DeserializeObject(eventMessage.JsonMessage, parameterType);

                    callback(messageObject, eventMessage.RoutingKey);
                }

                _busProvider.BasicConsume(queueName, ReceivedCallback);
            });
        }

        public Task SetupQueueListenerAsync<TParam>(
            string queueName,
            string topic,
            EventReceivedForTopic<TParam> callback)
        {
            EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));
            EnsureArg.IsNotNullOrWhiteSpace(topic, nameof(topic));
            EnsureArg.IsNotNull(callback, nameof(callback));

            return SetupQueueListenerAsync(
                queueName,
                topic,
                input => callback((TParam)input),
                typeof(TParam));
        }

        public Task SetupQueueListenerAsync(
            string queueName, 
            string topic,
            EventReceivedForTopic callback, 
            Type parameterType)
        {
            EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));
            EnsureArg.IsNotNullOrWhiteSpace(topic, nameof(topic));
            EnsureArg.IsNotNull(callback, nameof(callback));
            EnsureArg.IsNotNull(parameterType, nameof(parameterType));

            return Task.Run(() =>
            {
                void CallbackInvoker(string jsonParameter)
                {
                    var deserializedParamter = JsonConvert.DeserializeObject(jsonParameter, parameterType);

                    callback(deserializedParamter);
                }

                _callbackRegistry.AddCallbackForQueue(queueName, topic, CallbackInvoker);
            });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Remiworks.Core.Event.Matching;
using Remiworks.Core.Models;

namespace Remiworks.Core.Event
{
    public class EventListener : IEventListener
    {
        private readonly object _lockObject = new object();

        private readonly Dictionary<string, List<CallbackForTopic>> _queueCallbacks;
        private readonly IBusProvider _busProvider;

        public EventListener(IBusProvider busProvider)
        {
            _queueCallbacks = new Dictionary<string, List<CallbackForTopic>>();
            _busProvider = busProvider;
        }

        public async Task SetupQueueListener<TParam>(
            string queueName, 
            EventReceived<TParam> callback)
        {
            await Task.Run(() =>
            {
                void ReceivedCallback(EventMessage eventMessage)
                {
                    var messageObject = (TParam) JsonConvert.DeserializeObject(
                        eventMessage.JsonMessage,
                        typeof(TParam));

                    callback(messageObject, eventMessage.RoutingKey);
                }

                _busProvider.BasicConsume(queueName, ReceivedCallback);
            });
        }

        public async Task SetupQueueListener<TParam>(
            string queueName,
            string topic,
            EventReceivedForTopic<TParam> callback)
        {
            await Task.Run(() =>
            {
                void CallbackInvoker(string jsonParameter)
                {
                    var deserializedParamter = JsonConvert.DeserializeObject<TParam>(jsonParameter);

                    callback(deserializedParamter);
                }

                AddCallbackForQueue(queueName, topic, CallbackInvoker);
            });
        }

        private void AddCallbackForQueue(
            string queueName, 
            string topic, 
            Action<string> callbackInvoker)
        {
            var callbackForTopic = new CallbackForTopic
            {
                Topic = topic, 
                Callback = callbackInvoker
            };

            lock (_lockObject)
            {
                if (_queueCallbacks.ContainsKey(queueName))
                {
                    _queueCallbacks[queueName].Add(callbackForTopic);
                }
                else
                {
                    _queueCallbacks[queueName] = new List<CallbackForTopic> {callbackForTopic};
                    _busProvider.BasicConsume(
                        queueName, 
                        eventMessage =>
                            InvokeMatchingTopicCallbacks(eventMessage, _queueCallbacks[queueName]));
                }
            }
        }

        private static void InvokeMatchingTopicCallbacks(
            EventMessage eventMessage, 
            List<CallbackForTopic> callbacks)
        {
            var matchingTopics = TopicMatcher.Match(
                eventMessage.RoutingKey, 
                callbacks.Select(t => t.Topic).ToArray());

            callbacks
                .Where(t => matchingTopics.Contains(t.Topic))
                .ToList()
                .ForEach(t => t.Callback(eventMessage.JsonMessage));
        }
    }
}
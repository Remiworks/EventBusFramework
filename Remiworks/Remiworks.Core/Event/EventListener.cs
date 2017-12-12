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

        public Task SetupQueueListenerAsync<TParam>(string queueName, EventReceived<TParam> callback)
        {
            return SetupQueueListenerAsync(
                queueName, 
                (input, topic) => callback((TParam) input, topic), 
                typeof(TParam));
        }

        public Task SetupQueueListenerAsync(string queueName, EventReceived callback, Type type)
        {
            return Task.Run(() =>
            {
                void ReceivedCallback(EventMessage eventMessage)
                {
                    var messageObject = JsonConvert.DeserializeObject(eventMessage.JsonMessage, type);
                    
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
            return SetupQueueListenerAsync(
                queueName,
                topic,
                input => callback((TParam) input),
                typeof(TParam));
        }

        public Task SetupQueueListenerAsync(string queueName, string topic, EventReceivedForTopic callback, Type type)
        {
            return Task.Run(() =>
            {
                void CallbackInvoker(string jsonParameter)
                {
                    var deserializedParamter = JsonConvert.DeserializeObject(jsonParameter, type);

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
                _busProvider.CreateTopicsForQueue(queueName, topic);
                
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
                            InvokeMatchingTopicCallbacks(eventMessage, queueName));
                }
            }
        }

        private void InvokeMatchingTopicCallbacks(
            EventMessage eventMessage, 
            string queueName)
        {
            var matchingTopics = TopicMatcher.Match(
                eventMessage.RoutingKey, 
                _queueCallbacks[queueName].Select(t => t.Topic).ToArray());

            _queueCallbacks[queueName]
                .Where(t => matchingTopics.Contains(t.Topic))
                .ToList()
                .ForEach(t => t.Callback(eventMessage.JsonMessage));
        }
    }
}
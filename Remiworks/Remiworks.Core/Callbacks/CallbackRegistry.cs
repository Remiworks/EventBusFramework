using System;
using System.Collections.Generic;
using System.Linq;
using Remiworks.Core.Callbacks.Matching;
using Remiworks.Core.Models;

namespace Remiworks.Core.Callbacks
{
    public abstract class CallbackRegistry
    {
        private readonly object _lockObject = new object();
        private readonly IBusProvider _busProvider;
        private readonly Dictionary<string, List<CallbackForTopic>> _queueCallbacks;

        protected CallbackRegistry(IBusProvider busProvider)
        {
            _busProvider = busProvider;
            _queueCallbacks = new Dictionary<string, List<CallbackForTopic>>();
        }
        
        public void AddCallbackForQueue(
            string queueName,
            string topic,
            Action<string> callback)
        {
            var callbackForTopic = new CallbackForTopic
            {
                Topic = topic,
                Callback = callback
            };

            lock (_lockObject)
            {
                _busProvider.CreateTopicsForQueue(queueName, topic);

                if (_queueCallbacks.ContainsKey(queueName))
                {
                    if (CanAddCallback(_queueCallbacks[queueName], topic))
                    {   
                        _queueCallbacks[queueName].Add(callbackForTopic);
                    }
                }
                else
                {
                    _queueCallbacks[queueName] = new List<CallbackForTopic> { callbackForTopic };
                    _busProvider.BasicConsume(
                        queueName,
                        eventMessage => InvokeMatchingTopicCallbacks(eventMessage, queueName));
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

        protected abstract bool CanAddCallback(List<CallbackForTopic> registeredTopicsForQueue, string topicToAdd);
    }
}
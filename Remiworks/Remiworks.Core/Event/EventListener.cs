using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Remiworks.Core.Models;

namespace Remiworks.Core.Event
{
    public class EventListener : IEventListener
    {
        private readonly Dictionary<string, List<CallbackForTopic>> _queuesWithCallbacks;
        private readonly IBusProvider _busProvider;
        
        public EventListener(IBusProvider busProvider)
        {
            _queuesWithCallbacks = new Dictionary<string, List<CallbackForTopic>>();
            _busProvider = busProvider;
        }
        
        public async Task SetupQueueListener<TParam>(string queueName, EventReceived<TParam> callback)
        {
            await Task.Run(() =>
            {
                void ReceivedCallback(EventMessage eventMessage)
                {   
                    var messageObject = (TParam)JsonConvert.DeserializeObject(
                        eventMessage.JsonMessage, 
                        typeof(TParam));
    
                    callback(messageObject, eventMessage.RoutingKey);
                }
                
                _busProvider.BasicConsume(queueName, ReceivedCallback);
            });
        }

        public async Task SetupQueueListener<TParam>(string queueName, string topic, EventReceivedForTopic<TParam> callback)
        {
            await Task.Run(() =>
            {
                AddCallbackForTopic(queueName, topic, new Action(() =>
                {
                    
                }));
            });
        }

        private void AddCallbackForTopic(string queueName, string topic, Action callback)
        {
            var callbackForTopic = new CallbackForTopic
            {
                Topic = topic,
                Callback = callback
            };
            
            if (_queuesWithCallbacks.ContainsKey(queueName))
            {
                _queuesWithCallbacks[queueName].Add(callbackForTopic);
            }
            else
            {
                _queuesWithCallbacks[queueName] = new List<CallbackForTopic> { callbackForTopic };
            }
        }
    }

    public class CallbackForTopic
    {
        public string Topic { get; set; }
        public Action Callback { get; set; }
    }
}
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Remiworks.Core.Models;

namespace Remiworks.Core.Event
{
    public class EventListener : IEventListener
    {
        private readonly IBusProvider _busProvider;
        
        public EventListener(IBusProvider busProvider)
        {
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

        public Task SetupQueueListener<TParam>(string queueName, string topic, EventReceivedForTopic<TParam> callback)
        {
            throw new System.NotImplementedException();
        }
    }
}
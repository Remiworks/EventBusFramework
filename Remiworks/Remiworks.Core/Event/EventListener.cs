using System.Threading.Tasks;
using Newtonsoft.Json;

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
                _busProvider.BasicConsume(queueName, eventMessage =>
                {
                    var messageObject = (TParam)JsonConvert.DeserializeObject(
                        eventMessage.JsonMessage, 
                        typeof(TParam));
    
                    callback(messageObject, eventMessage.RoutingKey);
                }));
        }

        public Task SetupQueueListener<TParam>(string queueName, EventReceivedForTopic<TParam> callback)
        {
            throw new System.NotImplementedException();
        }
    }
}
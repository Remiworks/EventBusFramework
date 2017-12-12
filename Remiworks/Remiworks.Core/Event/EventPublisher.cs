using System.Threading.Tasks;
using Newtonsoft.Json;
using Remiworks.Core.Models;

namespace Remiworks.Core.Event
{
    public class EventPublisher : IEventPublisher
    {
        private readonly IBusProvider _busProvider;
        
        public EventPublisher(IBusProvider busProvider)
        {
            _busProvider = busProvider;
        }

        public async Task SendEventAsync(object message, string topic)
        {
            var eventMessage = new EventMessage
            {
                JsonMessage = JsonConvert.SerializeObject(message),
                RoutingKey = topic
            };

            await Task.Run(() =>
                _busProvider.BasicPublish(eventMessage));
        }
    }
}
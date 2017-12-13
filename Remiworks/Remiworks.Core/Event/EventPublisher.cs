using System.Threading.Tasks;
using EnsureThat;
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

        public Task SendEventAsync(object message, string topic)
        {
            EnsureArg.IsNotNull(message, nameof(message));
            EnsureArg.IsNotNullOrWhiteSpace(topic, nameof(topic));

            return Task.Run(() =>
            {
                var eventMessage = new EventMessage
                {
                    JsonMessage = JsonConvert.SerializeObject(message),
                    RoutingKey = topic
                };

                _busProvider.BasicPublish(eventMessage);
            });
        }
    }
}
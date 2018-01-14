using System.Threading.Tasks;
using EnsureThat;
using Newtonsoft.Json;
using Remiworks.Core.Models;

namespace Remiworks.Core.Event.Publisher
{
    public class EventPublisher : IEventPublisher
    {
        private readonly IBusProvider _busProvider;
        
        public EventPublisher(IBusProvider busProvider)
        {
            _busProvider = busProvider;

            _busProvider.EnsureConnection();
        }

        public Task SendEventAsync(object message, string routingKey)
        {
            EnsureArg.IsNotNull(message, nameof(message));
            EnsureArg.IsNotNullOrWhiteSpace(routingKey, nameof(routingKey));

            return Task.Run(() =>
            {
                var eventMessage = new EventMessage
                {
                    JsonMessage = JsonConvert.SerializeObject(message),
                    RoutingKey = routingKey
                };

                _busProvider.BasicPublish(eventMessage);
            });
        }
    }
}
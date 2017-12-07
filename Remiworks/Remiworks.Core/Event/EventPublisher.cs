using System.Threading.Tasks;

namespace Remiworks.Core.Event
{
    public class EventPublisher : IEventPublisher
    {
        private readonly IBusProvider _busProvider;
        
        public EventPublisher(IBusProvider busProvider)
        {
            _busProvider = busProvider;
        }

        public async Task BindTopicsToQueue(string queueName, params string[] topics)
        {
            await Task.Run(() => 
                _busProvider.CreateTopicsForQueue(queueName, topics));
        }

        public Task SendEvent(object message, string routingKey)
        {
            return null;
        }
    }
}
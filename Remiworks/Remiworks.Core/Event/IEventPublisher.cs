using System.Threading.Tasks;

namespace Remiworks.Core.Event
{
    public interface IEventPublisher
    {
        Task BindTopicsToQueueAsync(string queueName, params string[] topics);
        Task SendEventAsync(object message, string routingKey);
    }
}
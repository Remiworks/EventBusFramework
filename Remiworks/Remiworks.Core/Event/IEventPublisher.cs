using System.Threading.Tasks;

namespace Remiworks.Core.Event
{
    public interface IEventPublisher
    {
        Task BindTopicsToQueue(string queueName, params string[] topics);
        Task SendEvent(object message, string routingKey);
    }
}
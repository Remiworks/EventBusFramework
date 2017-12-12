using System.Threading.Tasks;

namespace Remiworks.Core.Event
{
    public interface IEventPublisher
    {
        Task SendEventAsync(object message, string routingKey);
    }
}
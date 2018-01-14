using System.Threading.Tasks;

namespace Remiworks.Core.Event.Publisher
{
    public interface IEventPublisher
    {
        Task SendEventAsync(object message, string routingKey);
    }
}
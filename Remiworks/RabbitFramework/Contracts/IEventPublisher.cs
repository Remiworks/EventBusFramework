using RabbitFramework.Models;

namespace RabbitFramework.Contracts
{
    public interface IEventPublisher
    {
        BusOptions BusOptions { get; set; }
    }
}
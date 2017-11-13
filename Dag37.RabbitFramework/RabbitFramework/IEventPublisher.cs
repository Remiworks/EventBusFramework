namespace RabbitFramework
{
    public interface IEventPublisher
    {
        IBusOptions BusOptions { get; set; }
    }
}
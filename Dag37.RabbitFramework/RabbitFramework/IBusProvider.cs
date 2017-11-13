using System;

namespace RabbitFramework
{
    public interface IBusProvider : IDisposable
    {
        IBusOptions BusOptions { get; }

        void CreateConnection();

        void BasicPublish(EventMessage message);

        void BasicConsume(string queueName, EventReceivedCallback callback);
    }

    public delegate void EventReceivedCallback(EventMessage message);
}
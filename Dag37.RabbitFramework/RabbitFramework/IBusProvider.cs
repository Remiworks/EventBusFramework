using System;
using System.Collections.Generic;

namespace RabbitFramework
{
    public interface IBusProvider : IDisposable
    {
        BusOptions BusOptions { get; }

        void CreateConnection();

        void BasicPublish(EventMessage message);

        void BasicConsume(string queueName, string topic, EventReceivedCallback callback);
    }

    public delegate void EventReceivedCallback(EventMessage message);
}
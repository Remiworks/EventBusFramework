using System;
using System.Collections.Generic;

namespace RabbitFramework
{
    public interface IBusProvider : IDisposable
    {
        BusOptions BusOptions { get; }

        void CreateConnection();

        void BasicPublish(EventMessage message);

        void CreateQueueWithTopics(string queueName, IEnumerable<string> topics);

        void BasicConsume(string queueName, EventReceivedCallback callback);
    }

    public delegate void EventReceivedCallback(EventMessage message);
}
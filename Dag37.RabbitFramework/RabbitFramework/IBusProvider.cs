using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitFramework
{
    public interface IBusProvider : IDisposable
    {
        BusOptions BusOptions { get; }

        void BasicPublish(EventMessage message);

        void CreateQueueWithTopics(string queueName, IEnumerable<string> topics);

        void BasicConsume(string queueName, EventReceivedCallback callback);

        Task<T> Call<T>(string queueName, object message, int timeout = 5000);

        void SetupRpcListener<TParam>(string queue, CommandReceivedCallback<TParam> function);
    }

    public delegate void EventReceivedCallback(EventMessage message);

    public delegate object CommandReceivedCallback<in TParam>(TParam command);
}
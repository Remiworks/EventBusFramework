using RabbitFramework.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitFramework.Contracts
{
    public interface IBusProvider : IDisposable
    {
        BusOptions BusOptions { get; }

        void CreateConnection();

        void BasicPublish(EventMessage eventMessage);

        void CreateQueueWithTopics(string queueName, IEnumerable<string> topics);

        void BasicConsume(string queueName, EventReceivedCallback callback);

        Task<T> Call<T>(string queueName, object message, int timeout = 5000);

        void SetupRpcListener<TParam>(string queueName, CommandReceivedCallback<TParam> function);
    }

    public delegate void EventReceivedCallback(EventMessage message);

    public delegate object CommandReceivedCallback<in TParam>(TParam command);
}
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

        void CreateTopicsForQueue(string queueName, params string[] topics);

        void BasicConsume(string queueName, EventReceivedCallback callback);
        
        void SetupRpcListener<TParam>(string queueName, CommandReceivedCallback<TParam> function);
        void SetupRpcListeners(string queueName, string[] keys, CommandReceivedCallback function);
    }

    public delegate void EventReceivedCallback(EventMessage message);

    public delegate CommandMessage CommandReceivedCallback(CommandMessage message);
}
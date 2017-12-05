using System;
using System.Threading.Tasks;
using Remiworks.Core.Models;

namespace Remiworks.Core
{
    public interface IBusProvider : IDisposable
    {
        BusOptions BusOptions { get; }

        void CreateConnection();

        void BasicPublish(EventMessage eventMessage);

        void CreateTopicsForQueue(string queueName, params string[] topics);

        void BasicConsume(string queueName, EventReceivedCallback callback);

        void SetupRpcListeners(string queueName, string[] keys, CommandReceivedCallback function);
    }

    public delegate void EventReceivedCallback(EventMessage message);

    public delegate Task<string> CommandReceivedCallback(EventMessage message);
}
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

        void BasicAcknowledge(ulong deliveryTag, bool multiple);
        
        void BasicTopicBind(string queueName, params string[] topics);

        void BasicConsume(string queueName, EventReceivedCallback callback, bool autoAcknowledge = true);

        void BasicQos(uint prefetchSize, ushort prefetchCount);
    }

    public delegate void EventReceivedCallback(EventMessage message);

    public delegate Task<string> CommandReceivedCallbackS(EventMessage message);
}
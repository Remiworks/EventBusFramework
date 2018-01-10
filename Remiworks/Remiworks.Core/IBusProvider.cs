using System;
using System.Threading.Tasks;
using Remiworks.Core.Models;

namespace Remiworks.Core
{
    public interface IBusProvider : IDisposable
    {
        BusOptions BusOptions { get; }

        void EnsureConnection();
        void BasicPublish(EventMessage eventMessage);
        void BasicPublish(EventMessage eventMessage, string exchangeName);

        void BasicAcknowledge(ulong deliveryTag, bool multiple);
        
        void BasicTopicBind(string queueName, string topic);

        void BasicTopicBind(string queueName, string topic, string exchangeName);

        void BasicConsume(string queueName, EventReceivedCallback callback, bool autoAcknowledge = true);

        void BasicQos(uint prefetchSize, ushort prefetchCount);
    }

    public delegate void EventReceivedCallback(EventMessage message);

    public delegate Task<string> CommandReceivedCallbackS(EventMessage message);
}
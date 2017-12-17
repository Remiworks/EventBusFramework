using System;
using Remiworks.Core.Models;

namespace Remiworks.Core.Event.Listener.Callbacks
{
    public interface IEventCallbackRegistry
    {
        void AddCallbackForQueue(string queueName, string topic, Action<EventMessage> callback);
    }
}
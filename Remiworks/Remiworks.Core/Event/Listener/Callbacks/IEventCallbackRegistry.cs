using System;

namespace Remiworks.Core.Event.Listener.Callbacks
{
    public interface IEventCallbackRegistry
    {
        void AddCallbackForQueue(string queueName, string topic, Action<string> callback);
    }
}
using System;
using Remiworks.Core.Models;

namespace Remiworks.Core.Command.Listener.Callbacks
{
    public interface ICommandCallbackRegistry
    {
        void AddCallbackForQueue(string queueName, string topic, Action<EventMessage> callback);
    }
}
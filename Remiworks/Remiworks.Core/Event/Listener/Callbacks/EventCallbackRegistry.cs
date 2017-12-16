using System.Collections.Generic;
using Remiworks.Core.Callbacks;
using Remiworks.Core.Models;

namespace Remiworks.Core.Event.Listener.Callbacks
{
    public class EventCallbackRegistry : CallbackRegistry, IEventCallbackRegistry
    {
        public EventCallbackRegistry(IBusProvider busProvider) : base(busProvider)
        {
        }

        protected override bool CanAddCallback(List<CallbackForTopic> registeredTopicsForQueue, string topicToAdd)
        {
            return true;
        }
    }
}
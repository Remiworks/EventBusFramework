using System.Collections.Generic;
using Remiworks.Core.Callbacks;
using Remiworks.Core.Models;

namespace Remiworks.Core.Event.Listener.Callbacks
{
    public class EventCallbackRegistry : CallbackRegistry, IEventCallbackRegistry
    {
        private readonly IBusProvider _busProvider;
        
        public EventCallbackRegistry(IBusProvider busProvider) : base(busProvider)
        {
            _busProvider = busProvider;
        }

        protected override bool CanAddCallback(IEnumerable<CallbackForTopic> registeredTopicsForQueue, string topicToAdd)
        {
            return true;
        }

        protected override void RegisterCallbackListener(string queueName, EventReceivedCallback callback)
        {
            _busProvider.BasicConsume(queueName, callback);
        }
    }
}
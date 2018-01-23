using System.Collections.Generic;
using System.Linq;
using Remiworks.Core.Callbacks;
using Remiworks.Core.Models;

namespace Remiworks.Core.Command.Listener.Callbacks
{
    public class CommandCallbackRegistry : CallbackRegistry, ICommandCallbackRegistry
    {
        private readonly IBusProvider _busProvider;
        
        public CommandCallbackRegistry(IBusProvider busProvider) : base(busProvider)
        {
            _busProvider = busProvider;
        }

        protected override void RegisterCallbackListener(string queueName, EventReceivedCallback callback)
        {
            _busProvider.BasicConsume(
                queueName: queueName,
                callback: callback,
                autoAcknowledge: false);
            
            _busProvider.BasicQos(0, 1);
        }

        protected override bool CanAddCallback(IEnumerable<CallbackForTopic> registeredTopicsForQueue, string topicToAdd)
        {
            return registeredTopicsForQueue.All(t => t.Topic != topicToAdd);
        }
    }
}
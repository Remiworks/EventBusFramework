using System.Collections.Generic;
using System.Linq;
using Remiworks.Core.Callbacks;
using Remiworks.Core.Models;

namespace Remiworks.Core.Command.Listener.Callbacks
{
    public class CommandCallbackRegistry : CallbackRegistry, ICommandCallbackRegistry
    {
        public CommandCallbackRegistry(IBusProvider busProvider) : base(busProvider)
        {
        }

        protected override bool CanAddCallback(List<CallbackForTopic> registeredTopicsForQueue, string topicToAdd)
        {
            return !registeredTopicsForQueue.Any(t => t.Topic == topicToAdd);
        }
    }
}
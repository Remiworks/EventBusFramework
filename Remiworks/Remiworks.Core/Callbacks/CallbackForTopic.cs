using System;
using Remiworks.Core.Models;

namespace Remiworks.Core.Callbacks
{
    public class CallbackForTopic
    {
        public string Topic { get; set; }
        public Action<EventMessage> Callback { get; set; }
    }
}
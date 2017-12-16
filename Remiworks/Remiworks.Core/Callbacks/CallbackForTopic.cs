using System;

namespace Remiworks.Core.Callbacks
{
    public class CallbackForTopic
    {
        public string Topic { get; set; }
        public Action<string> Callback { get; set; }
    }
}
using System;

namespace Remiworks.Core.Models
{
    internal class CallbackForTopic
    {
        public string Topic { get; set; }
        public Action<string> Callback { get; set; }
    }
}
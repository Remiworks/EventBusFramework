using System;

namespace Remiworks.Core.Models
{
    public class CallbackForTopic
    {
        public string Topic { get; set; }
        public Action<string> Callback { get; set; }
    }
}
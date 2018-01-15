using System;

namespace Remiworks.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EventAttribute : ListenerAttribute
    {
        public EventAttribute(string topic, string exchangeName = null) : base (topic, exchangeName)
        {
        }
    }
}
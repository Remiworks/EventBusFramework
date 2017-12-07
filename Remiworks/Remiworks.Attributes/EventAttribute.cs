using System;

namespace Remiworks.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EventAttribute : Attribute
    {
        public string Topic { get; }

        public EventAttribute(string topic)
        {
            Topic = topic;
        }
    }
}
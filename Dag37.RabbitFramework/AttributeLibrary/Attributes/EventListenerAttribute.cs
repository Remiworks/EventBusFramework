using System;

namespace AttributeLibrary.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EventListenerAttribute : Attribute
    {
        public string QueueName { get; }

        public EventListenerAttribute(string queueName)
        {
            QueueName = queueName;
        }
    }
}
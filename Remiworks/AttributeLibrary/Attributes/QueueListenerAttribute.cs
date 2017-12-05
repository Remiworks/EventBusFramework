using System;

namespace AttributeLibrary.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class QueueListenerAttribute : Attribute
    {
        public string QueueName { get; }

        public QueueListenerAttribute(string queueName)
        {
            QueueName = queueName;
        }
    }
}
using System;

namespace Remiworks.Attributes
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
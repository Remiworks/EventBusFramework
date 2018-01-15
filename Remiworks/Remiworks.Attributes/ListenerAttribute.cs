using System;

namespace Remiworks.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class ListenerAttribute : Attribute
    {
        public string Topic { get; }
        public string ExchangeName { get; }

        protected ListenerAttribute(string topic, string exchangeName)
        {
            Topic = topic;
            ExchangeName = exchangeName;
        }
    }
}
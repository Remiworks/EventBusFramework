using System;

namespace AttributeLibrary.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TopicAttribute : Attribute
    {
        public string Topic { get; }

        public TopicAttribute(string topic)
        {
            Topic = topic;
        }
    }
}
using System;

namespace AttributeLibrary
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
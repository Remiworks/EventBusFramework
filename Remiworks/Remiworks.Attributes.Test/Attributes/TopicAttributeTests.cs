using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Remiworks.Attributes.Test.Attributes
{
    [TestClass]
    public class TopicAttributeTests
    {
        [TestMethod]
        public void TopicAttributeMustBeInitializeWithTopic()
        {
            string topic = "topic";

            EventAttribute attribute = new EventAttribute(topic);

            attribute.Topic.ShouldBe(topic);
        }

        [TestMethod]
        public void TopicAttributeHasAttributeUsageAnnotation()
        {
            var type = typeof(EventAttribute);

            var attributes = type.GetCustomAttributes(false);

            var attribute = attributes.FirstOrDefault();

            attribute.ShouldBeOfType<AttributeUsageAttribute>();

            (attribute as AttributeUsageAttribute).ValidOn.ShouldBe(AttributeTargets.Method);
        }
    }
}
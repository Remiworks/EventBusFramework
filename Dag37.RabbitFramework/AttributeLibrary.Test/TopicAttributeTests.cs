using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AttributeLibrary.Test
{
    [TestClass]
    public class TopicAttributeTests
    {
        [TestMethod]
        public void TopicAttributeMustBeInitializeWithTopic()
        {
            string topic = "topic";

            TopicAttribute attribute = new TopicAttribute(topic);

            attribute.Topic.ShouldBe(topic);
        }

        [TestMethod]
        public void TopicAttributeHasAttributeUsageAnnotation()
        {
            var type = typeof(TopicAttribute);

            var attributes = type.GetCustomAttributes(false);

            var attribute = attributes.FirstOrDefault();

            attribute.ShouldBeOfType<AttributeUsageAttribute>();

            (attribute as AttributeUsageAttribute).ValidOn.ShouldBe(AttributeTargets.Method);
        }
    }
}

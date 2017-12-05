using AttributeLibrary.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Linq;

namespace AttributeLibrary.Test.Attributes
{
    [TestClass]
    public class EventListenerAttributeTests
    {
        [TestMethod]
        public void EventListenerAttributeMustInitializeWithQueueName()
        {
            string queueName = "queueName";
            QueueListenerAttribute attribute = new QueueListenerAttribute(queueName);

            attribute.QueueName.ShouldBe(queueName);
        }

        [TestMethod]
        public void EventListenerHasAttributeUsageAnnotation()
        {
            var type = typeof(QueueListenerAttribute);

            var attributes = type.GetCustomAttributes(false);

            var attribute = attributes.FirstOrDefault();

            attribute.ShouldBeOfType<AttributeUsageAttribute>();

            (attribute as AttributeUsageAttribute).ValidOn.ShouldBe(AttributeTargets.Class);
        }
    }
}
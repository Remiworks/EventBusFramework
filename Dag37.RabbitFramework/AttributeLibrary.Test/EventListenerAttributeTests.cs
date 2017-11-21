using AttributeLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabbitFramework.Test
{
    [TestClass]
    public class EventListenerAttributeTests
    {
        [TestMethod]
        public void EventListenerAttributeMustInitializeWithQueueName()
        {
            string queueName = "queueName";
            EventListenerAttribute attribute = new EventListenerAttribute(queueName);

            attribute.QueueName.ShouldBe(queueName);
        }

        [TestMethod]
        public void EventListenerHasAttributeUsageAnnotation()
        {
            var type = typeof(EventListenerAttribute);

            var attributes = type.GetCustomAttributes(false);

            var attribute = attributes.FirstOrDefault();

            attribute.ShouldBeOfType<AttributeUsageAttribute>();

            (attribute as AttributeUsageAttribute).ValidOn.ShouldBe(AttributeTargets.Class);
        }
    }
}

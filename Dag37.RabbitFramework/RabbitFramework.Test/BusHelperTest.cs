using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitFramework.Test
{
    [TestClass]
    public class BusHelperTest
    {
        [TestMethod]
        public void GenerateQueueNameByQueueAndTopic()
        {
            var queue = "user";
            var topic = "event.created";

            IBusHelper target = new BusHelper();

            var result = target.GenerateQueueName(queue, topic);

            result.ShouldBe("user-event-created");
        }

        [TestMethod]
        public void GenerateTopicNameByQueueAndTopic()
        {
            var queue = "user";
            var topic = "event.created";

            IBusHelper target = new BusHelper();

            var result = target.GenerateTopicName(queue, topic);

            result.ShouldBe("user.event.created");
        }
    }
}

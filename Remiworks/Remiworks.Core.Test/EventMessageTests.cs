using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Remiworks.Core.Models;
using Shouldly;

namespace RabbitFramework.Test
{
    [TestClass]
    public class EventMessageTests
    {
        private EventMessage _sut;

        [TestInitialize]
        public void TestInitialize()
        {
            _sut = new EventMessage();
        }

        [TestMethod]
        public void RoutingKeyCanBeSetAndGet()
        {
            string routingKey = "Some value";

            _sut.RoutingKey = routingKey;
            _sut.RoutingKey.ShouldBe(routingKey);
        }

        [TestMethod]
        public void CorrelationIdCanBeSetAndGet()
        {
            Guid? correlationId = new Guid("bb74dc72-9896-4abd-b628-8798b810ee7f");

            _sut.CorrelationId = correlationId;
            _sut.CorrelationId.ShouldBe(correlationId);
        }

        [TestMethod]
        public void TimestampCanBeSetAndGet()
        {
            long timestamp = 12345678L;

            _sut.Timestamp = timestamp;
            _sut.Timestamp.ShouldBe(timestamp);
        }

        [TestMethod]
        public void ReplyQueueNameCanBeSetAndGet()
        {
            string replyQueueName = "Some value";

            _sut.ReplyQueueName = replyQueueName;
            _sut.ReplyQueueName.ShouldBe(replyQueueName);
        }

        [TestMethod]
        public void TypeCanBeSetAndGet()
        {
            string type = "Some value";

            _sut.Type = type;
            _sut.Type.ShouldBe(type);
        }

        [TestMethod]
        public void JsonMessageCanBeSetAndGet()
        {
            string jsonMessage = "Some value";

            _sut.JsonMessage = jsonMessage;
            _sut.JsonMessage.ShouldBe(jsonMessage);
        }
    }
}
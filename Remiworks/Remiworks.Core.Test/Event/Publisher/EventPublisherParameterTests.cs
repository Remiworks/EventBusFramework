using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Remiworks.Core.Event.Publisher;
using Remiworks.Core.Test.Stubs;
using Shouldly;

namespace Remiworks.Core.Test.Event.Publisher
{
    [TestClass]
    public class EventPublisherParameterTests
    {
        private const string MessageParameter = "message";
        private const string RoutingKeyParameter = "routingKey";

        private const string Topic = "test.topic";

        private static readonly Person Message = new Person { Name = "Jan Janssen" };

        private EventPublisher _sut;

        [TestInitialize]
        public void Initialize()
        {
            _sut = new EventPublisher(new Mock<IBusProvider>().Object);
        }

        [TestMethod]
        public void SendEventAsyncThrows_ArgumentNullException_WhenMessageIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
                _sut.SendEventAsync(null, Topic)
                    .Wait());

            exception.ParamName.ShouldBe(MessageParameter);
        }

        [TestMethod]
        public void SendEventAsyncThrows_ArgumentNullException_WhenRoutingKeyIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
                _sut.SendEventAsync(Message, null)
                    .Wait());

            exception.ParamName.ShouldBe(RoutingKeyParameter);
        }

        [TestMethod]
        public void SendEventAsyncThrows_ArgumentException_WhenRoutingKeyIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SendEventAsync(Message, "")
                    .Wait());

            exception.ParamName.ShouldBe(RoutingKeyParameter);
        }

        [TestMethod]
        public void SendEventAsyncThrows_ArgumentException_WhenRoutingKeyIsWhiteSpace()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SendEventAsync(Message, " ")
                    .Wait());

            exception.ParamName.ShouldBe(RoutingKeyParameter);
        }
    }
}
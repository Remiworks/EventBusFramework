using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Remiworks.Core.Event;
using Remiworks.Core.Test.Event.Stubs;
using Remiworks.Core.Test.Stubs;
using Shouldly;

namespace Remiworks.Core.Test.Event
{
    [TestClass]
    public class EventPublisherParameterTests
    {
        private const string MessageParameter = "message";
        private const string TopicParameter = "topic";

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
        public void SendEventAsyncThrows_ArgumentNullException_WhenTopicIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
                _sut.SendEventAsync(Message, null)
                    .Wait());

            exception.ParamName.ShouldBe(TopicParameter);
        }

        [TestMethod]
        public void SendEventAsyncThrows_ArgumentException_WhenTopicIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SendEventAsync(Message, "")
                    .Wait());

            exception.ParamName.ShouldBe(TopicParameter);
        }

        [TestMethod]
        public void SendEventAsyncThrows_ArgumentException_WhenTopicIsWhiteSpace()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SendEventAsync(Message, " ")
                    .Wait());

            exception.ParamName.ShouldBe(TopicParameter);
        }
    }
}
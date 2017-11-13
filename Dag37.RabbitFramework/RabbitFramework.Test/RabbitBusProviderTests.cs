using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;
using System;

namespace RabbitFramework.Test
{
    [TestClass]
    public class RabbitBusProviderTests
    {
        private readonly Mock<BusOptions> _busOptionsMock = new Mock<BusOptions>();

        private RabbitBusProvider _sut;

        [TestInitialize]
        public void Initialize()
        {
            _sut = new RabbitBusProvider(_busOptionsMock.Object);
        }

        [TestMethod]
        public void ConstructorSetsBusOptions()
        {
            _sut.BusOptions.ShouldBe(_busOptionsMock.Object);
        }

        [TestMethod]
        public void BasicConsumeThrowsArgumentExceptionWhenQueueNameIsNull()
        {
            EventReceivedCallback callback = new EventReceivedCallback((message) => { });

            var exception = Should.Throw<ArgumentException>(() => _sut.BasicConsume(null, callback));
            exception.Message.ShouldBe("queueName");
        }

        [TestMethod]
        public void BasicConsumeThrowsArgumentExceptionWhenQueueNameIsEmpty()
        {
            EventReceivedCallback callback = new EventReceivedCallback((message) => { });

            var exception = Should.Throw<ArgumentException>(() => _sut.BasicConsume("", callback));
            exception.Message.ShouldBe("queueName");
        }

        [TestMethod]
        public void BasicConsumeThrowsArgumentExceptionWhenQueueNameIsWhiteSpace()
        {
            EventReceivedCallback callback = new EventReceivedCallback((message) => { });

            var exception = Should.Throw<ArgumentException>(() => _sut.BasicConsume(" ", callback));
            exception.Message.ShouldBe("queueName");
        }

        [TestMethod]
        public void BasicConsumeThrowsArgumentExceptionWhenCallbackIsNull()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.BasicConsume("SomeQueue", null));
            exception.Message.ShouldBe("callback");
        }

        [TestMethod]
        public void BasicConsumeThrowsArgumentExceptionWhenTopicIsNull()
        {
            EventReceivedCallback callback = new EventReceivedCallback((message) => { });

            var exception = Should.Throw<ArgumentException>(() => _sut.BasicConsume("queue", null, callback));
            exception.Message.ShouldBe("topic");
        }

        [TestMethod]
        public void BasicConsumeThrowsArgumentExceptionWhenTopicIsEmpty()
        {
            EventReceivedCallback callback = new EventReceivedCallback((message) => { });

            var exception = Should.Throw<ArgumentException>(() => _sut.BasicConsume("queue", "", callback));
            exception.Message.ShouldBe("topic");
        }

        [TestMethod]
        public void BasicConsumeThrowsArgumentExceptionWhenTopicIsWhiteSpace()
        {
            EventReceivedCallback callback = new EventReceivedCallback((message) => { });

            var exception = Should.Throw<ArgumentException>(() => _sut.BasicConsume("queue", " ", callback));
            exception.Message.ShouldBe("topic");
        }
    }
}
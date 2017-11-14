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
        private readonly Mock<IBusHelper> _busHelperMock = new Mock<IBusHelper>();

        private RabbitBusProvider _sut;

        [TestInitialize]
        public void Initialize()
        {
            _sut = new RabbitBusProvider(_busOptionsMock.Object, _busHelperMock.Object);
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

            var exception = Should.Throw<ArgumentException>(() => _sut.BasicConsume(null, "test", callback));
            exception.Message.ShouldBe("Queue is required");
        }

        [TestMethod]
        public void BasicConsumeThrowsArgumentExceptionWhenQueueNameIsEmpty()
        {
            EventReceivedCallback callback = new EventReceivedCallback((message) => { });

            var exception = Should.Throw<ArgumentException>(() => _sut.BasicConsume("", "test", callback));
            exception.Message.ShouldBe("Queue is required");
        }

        [TestMethod]
        public void BasicConsumeThrowsArgumentExceptionWhenQueueNameIsWhiteSpace()
        {
            EventReceivedCallback callback = new EventReceivedCallback((message) => { });

            var exception = Should.Throw<ArgumentException>(() => _sut.BasicConsume(" ", "test", callback));
            exception.Message.ShouldBe("Queue is required");
        }

        [TestMethod]
        public void BasicConsumeThrowsArgumentExceptionWhenCallbackIsNull()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.BasicConsume("SomeQueue", "test", null));
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
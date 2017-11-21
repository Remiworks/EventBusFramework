using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;

namespace RabbitFramework.Test
{
    [TestClass]
    public class RabbitBusProviderTests
    {
        private const string TopicsParamName = "topics";
        private const string CallbackParamName = "callback";
        private const string QueueNameParamName = "queueName";
        private const string EventMessageParamName = "eventMessage";
        private const string FunctionParamName = "function";
        private const string MessageParamName = "message";

        private readonly IEnumerable<string> _topics = new List<string> { "SomeTopic1", "SomeTopic2" };
        private readonly Mock<BusOptions> _busOptionsMock = new Mock<BusOptions>();
        private readonly CommandReceivedCallback<string> _commandReceivedCallback = (p) => { return new object(); };

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
            exception.Message.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void BasicConsumeThrowsArgumentExceptionWhenQueueNameIsEmpty()
        {
            EventReceivedCallback callback = new EventReceivedCallback((message) => { });

            var exception = Should.Throw<ArgumentException>(() => _sut.BasicConsume("", callback));
            exception.Message.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void BasicConsumeThrowsArgumentExceptionWhenQueueNameIsWhiteSpace()
        {
            EventReceivedCallback callback = new EventReceivedCallback((message) => { });

            var exception = Should.Throw<ArgumentException>(() => _sut.BasicConsume(" ", callback));
            exception.Message.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void BasicConsumeThrowsArgumentExceptionWhenCallbackIsNull()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.BasicConsume("SomeQueue", null));
            exception.Message.ShouldBe(CallbackParamName);
        }

        [TestMethod]
        public void CreateQueueWithTopicsThrowsArgumentExceptionWhenQueueNameIsNull()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.CreateQueueWithTopics(null, _topics));
            exception.Message.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void CreateQueueWithTopicsThrowsArgumentExceptionWhenQueueNameIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.CreateQueueWithTopics("", _topics));
            exception.Message.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void CreateQueueWithTopicsThrowsArgumentExceptionWhenQueueNameIsWhiteSpace()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.CreateQueueWithTopics(" ", _topics));
            exception.Message.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void CreateQueueWithTopicsThrowsArgumentExceptionWhenTopicsIsNull()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.CreateQueueWithTopics("SomeQueue", null));
            exception.Message.ShouldBe(TopicsParamName);
        }

        [TestMethod]
        public void CreateQueueWithTopicsThrowsArgumentExceptionWhenTopicsIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.CreateQueueWithTopics("SomeQueue", new List<string>()));
            exception.Message.ShouldBe(TopicsParamName);
        }

        [TestMethod]
        public void BasicPublishThrowsArgumentExceptionWhenMessageIsNull()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.BasicPublish(null));
            exception.Message.ShouldBe(EventMessageParamName);
        }

        [TestMethod]
        public void SetupRpcListenerThrowsArgumentExceptionWhenQueueNameIsNull()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.SetupRpcListener(null, _commandReceivedCallback));
            exception.Message.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void SetupRpcListenerThrowsArgumentExceptionWhenQueueNameIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.SetupRpcListener("", _commandReceivedCallback));
            exception.Message.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void SetupRpcListenerThrowsArgumentExceptionWhenQueueNameIsWhitespace()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.SetupRpcListener(" ", _commandReceivedCallback));
            exception.Message.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void SetupRpcListenerThrowsArgumentExceptionWhenFuntionIsNull()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.SetupRpcListener<string>("SomeQueue", null));
            exception.Message.ShouldBe(FunctionParamName);
        }

        [TestMethod]
        public void CallThrowsArgumentExceptionWhenQueueNameIsNull()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.Call<string>(null, new object(), 0));
            exception.Message.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void CallThrowsArgumentExceptionWhenQueueNameIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.Call<string>("", new object(), 0));
            exception.Message.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void CallThrowsArgumentExceptionWhenQueueNameIsWhitespace()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.Call<string>(" ", new object(), 0));
            exception.Message.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void CallThrowsArgumentExceptionWhenMessageIsNull()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.Call<string>("SomeQueue", null, 0));
            exception.Message.ShouldBe(MessageParamName);
        }
    }
}
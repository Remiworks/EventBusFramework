using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Remiworks.Core.Event;
using Remiworks.Core.Event.Listener;
using Remiworks.Core.Event.Listener.Callbacks;
using Remiworks.Core.Test.Stubs;
using Shouldly;

namespace Remiworks.Core.Test.Event.Listener
{
    [TestClass]
    public class EventListenerParameterTests
    {
        private const string QueueNameParameter = "queueName";
        private const string CallbackParameter = "callback";
        private const string TypeParameter = "parameterType";
        private const string TopicParameter = "topic";

        private const string QueueName = "testQueue";
        private const string Topic = "test.topic";

        private EventListener _sut;

        [TestInitialize]
        public void Initialize()
        {
            _sut = new EventListener(new Mock<IBusProvider>().Object, new Mock<IEventCallbackRegistry>().Object);
        }

        [TestMethod]
        public void ListenToQueueGenericThrows_ArgumentNullException_WhenQueueNameIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
                _sut.SetupQueueListenerAsync(null, new Mock<EventReceived<Person>>().Object).Wait());

            exception.ParamName.ShouldBe(QueueNameParameter);
        }

        [TestMethod]
        public void ListenToQueueGenericThrows_ArgumentException_WhenQueueNameIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupQueueListenerAsync("", new Mock<EventReceived<Person>>().Object)
                    .Wait());

            exception.ParamName.ShouldBe(QueueNameParameter);
        }

        [TestMethod]
        public void ListenToQueueGenericThrows_ArgumentException_WhenQueueNameIsWhitespace()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupQueueListenerAsync(" ", new Mock<EventReceived<Person>>().Object).Wait());

            exception.ParamName.ShouldBe(QueueNameParameter);
        }

        [TestMethod]
        public void ListenToQueueGenericThrows_ArgumentNullException_WhenCallbackIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
                _sut.SetupQueueListenerAsync<Person>(QueueName, null).Wait());

            exception.ParamName.ShouldBe(CallbackParameter);
        }

        [TestMethod]
        public void ListenToQueueNonGenericThrows_ArgumentNullException_WhenQueueNameIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
                _sut.SetupQueueListenerAsync(null, new Mock<EventReceived>().Object, new Mock<Type>().Object)
                    .Wait());

            exception.ParamName.ShouldBe(QueueNameParameter);
        }

        [TestMethod]
        public void ListenToQueueNonGenericThrows_ArgumentException_WhenQueueNameIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupQueueListenerAsync("", new Mock<EventReceived>().Object, new Mock<Type>().Object)
                    .Wait());

            exception.ParamName.ShouldBe(QueueNameParameter);
        }

        [TestMethod]
        public void ListenToQueueNonGenericThrows_ArgumentException_WhenQueueNameIsWhitespace()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupQueueListenerAsync(" ", new Mock<EventReceived>().Object, new Mock<Type>().Object)
                    .Wait());

            exception.ParamName.ShouldBe(QueueNameParameter);
        }

        [TestMethod]
        public void ListenToQueueNonGenericThrows_ArgumentNullException_WhenCallbackIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
                _sut.SetupQueueListenerAsync(QueueName, null, new Mock<Type>().Object)
                    .Wait());

            exception.ParamName.ShouldBe(CallbackParameter);
        }

        [TestMethod]
        public void ListenToQueueNonGenericThrows_ArgumentNullException_WhenTypeIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
                _sut.SetupQueueListenerAsync(QueueName, new Mock<EventReceived>().Object, null)
                    .Wait());

            exception.ParamName.ShouldBe(TypeParameter);
        }
        
        [TestMethod]
        public void ListenToQueueGenericOverloadThrows_ArgumentNullException_WhenQueueNameIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
                _sut.SetupQueueListenerAsync(null, Topic, new Mock<EventReceivedForTopic<Person>>().Object).Wait());

            exception.ParamName.ShouldBe(QueueNameParameter);
        }

        [TestMethod]
        public void ListenToQueueGenericOverloadThrows_ArgumentException_WhenQueueNameIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupQueueListenerAsync("", Topic, new Mock<EventReceivedForTopic<Person>>().Object)
                    .Wait());

            exception.ParamName.ShouldBe(QueueNameParameter);
        }

        [TestMethod]
        public void ListenToQueueGenericOverloadThrows_ArgumentException_WhenQueueNameIsWhitespace()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupQueueListenerAsync(" ", Topic, new Mock<EventReceivedForTopic<Person>>().Object).Wait());

            exception.ParamName.ShouldBe(QueueNameParameter);
        }
        
        [TestMethod]
        public void ListenToQueueGenericOverloadThrows_ArgumentNullException_WhenTopicIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
                _sut.SetupQueueListenerAsync(QueueName, null, new Mock<EventReceivedForTopic<Person>>().Object)
                    .Wait());

            exception.ParamName.ShouldBe(TopicParameter);
        }

        [TestMethod]
        public void ListenToQueueGenericOverloadThrows_ArgumentException_WhenTopicIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupQueueListenerAsync(QueueName, "", new Mock<EventReceivedForTopic<Person>>().Object)
                    .Wait());

            exception.ParamName.ShouldBe(TopicParameter);
        }

        [TestMethod]
        public void ListenToQueueGenericOverloadThrows_ArgumentException_WhenTopicIsWhitespace()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupQueueListenerAsync(QueueName, " ", new Mock<EventReceivedForTopic<Person>>().Object)
                    .Wait());

            exception.ParamName.ShouldBe(TopicParameter);
        }
        
        [TestMethod]
        public void ListenToQueueGenericOverloadThrows_ArgumentNullException_WhenCallbackIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
                _sut.SetupQueueListenerAsync<Person>(QueueName, Topic, null).Wait());

            exception.ParamName.ShouldBe(CallbackParameter);
        }

        [TestMethod]
        public void ListenToQueueNonGenericOverloadThrows_ArgumentNullException_WhenQueueNameIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
                _sut.SetupQueueListenerAsync(null, Topic, new Mock<EventReceivedForTopic>().Object, new Mock<Type>().Object)
                    .Wait());

            exception.ParamName.ShouldBe(QueueNameParameter);
        }

        [TestMethod]
        public void ListenToQueueNonGenericOverloadThrows_ArgumentException_WhenQueueNameIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupQueueListenerAsync("", Topic, new Mock<EventReceivedForTopic>().Object, new Mock<Type>().Object)
                    .Wait());

            exception.ParamName.ShouldBe(QueueNameParameter);
        }

        [TestMethod]
        public void ListenToQueueNonGenericOverloadThrows_ArgumentException_WhenQueueNameIsWhitespace()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupQueueListenerAsync(" ", Topic, new Mock<EventReceivedForTopic>().Object, new Mock<Type>().Object)
                    .Wait());

            exception.ParamName.ShouldBe(QueueNameParameter);
        }

        [TestMethod]
        public void ListenToQueueNonGenericOverloadThrows_ArgumentNullException_WhenTopicIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
                _sut.SetupQueueListenerAsync(QueueName, null, new Mock<EventReceivedForTopic>().Object, new Mock<Type>().Object)
                    .Wait());

            exception.ParamName.ShouldBe(TopicParameter);
        }

        [TestMethod]
        public void ListenToQueueNonGenericOverloadThrows_ArgumentException_WhenTopicIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupQueueListenerAsync(QueueName, "", new Mock<EventReceivedForTopic>().Object, new Mock<Type>().Object)
                    .Wait());

            exception.ParamName.ShouldBe(TopicParameter);
        }

        [TestMethod]
        public void ListenToQueueNonGenericOverloadThrows_ArgumentException_WhenTopicIsWhitespace()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupQueueListenerAsync(QueueName, " ", new Mock<EventReceivedForTopic>().Object, new Mock<Type>().Object)
                    .Wait());

            exception.ParamName.ShouldBe(TopicParameter);
        }

        [TestMethod]
        public void ListenToQueueNonGenericOverloadThrows_ArgumentNullException_WhenCallbackIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
                _sut.SetupQueueListenerAsync(QueueName, Topic, null, new Mock<Type>().Object)
                    .Wait());

            exception.ParamName.ShouldBe(CallbackParameter);
        }

        [TestMethod]
        public void ListenToQueueNonGenericOverloadThrows_ArgumentNullException_WhenTypeIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
                _sut.SetupQueueListenerAsync(QueueName, Topic, new Mock<EventReceivedForTopic>().Object, null)
                    .Wait());

            exception.ParamName.ShouldBe(TypeParameter);
        }
    }
}
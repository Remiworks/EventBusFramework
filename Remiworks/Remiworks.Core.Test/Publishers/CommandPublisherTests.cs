using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Remiworks.Core.Command;
using Remiworks.Core.Models;
using Shouldly;

namespace Remiworks.Core.Test.Publishers
{
    [TestClass]
    public class CommandPublisherTests
    {
        private const string WildcardExceptionMessage = "Key shouldn't contain wildcards";
        private const string QueueNameParamName = "queueName";
        private const string MessageParamName = "message";
        private const string KeyParamName = "key";

        private const string Message = "testMessage";
        private const string Key = "testKey";
        private const string QueueName = "testQueue";
        private const int Timeout = 0;

        private readonly Mock<IBusProvider> _busProviderMock = new Mock<IBusProvider>(MockBehavior.Strict);

        private CommandPublisher _sut;

        private EventReceivedCallback _eventReceivedCallback = (_) =>
            throw new NotImplementedException("This callback is not yet set by IBusProvider.BasicConsume");

        [TestInitialize]
        public void TestInitialize()
        {
            _busProviderMock
                .Setup(b => b.BasicConsume(It.IsAny<string>(), It.IsAny<EventReceivedCallback>()))
                .Callback<string, EventReceivedCallback>((_, callback) => _eventReceivedCallback = callback);

            _sut = new CommandPublisher(_busProviderMock.Object);
        }

        [TestMethod]
        public async void SendCommandCallsBasicPublishWithCorrectParameters()
        {
            _busProviderMock
                .Setup(b => b.CreateTopicsForQueue(It.IsAny<string>(), It.IsAny<string[]>()));

            _busProviderMock
                .Setup(b => b.BasicPublish(It.Is(CorrectEventMessage)))
                .Callback(BasicPublishCallback)
                .Verifiable();

            await _sut.SendCommandAsync<string>(Message, QueueName, Key);

            _busProviderMock.VerifyAll();
        }

        [TestMethod]
        public async void SendCommandReturnsCorrectResult()
        {
            _busProviderMock
               .Setup(b => b.CreateTopicsForQueue(It.IsAny<string>(), It.IsAny<string[]>()));

            _busProviderMock
                .Setup(b => b.BasicPublish(It.Is(CorrectEventMessage)))
                .Callback(BasicPublishCallback)
                .Verifiable();

            var result = await _sut.SendCommandAsync<string>(Message, QueueName, Key);

            result.ShouldBe(Message.Reverse().ToString());
        }

        [TestMethod]
        public void SendCommandThrows_ArgumentNullException_WhenQueueNameIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() => _sut.SendCommandAsync<string>(new object(), null, Key, Timeout));
            exception.ParamName.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void SendCommandThrows_ArgumentException_WhenQueueNameIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.SendCommandAsync<string>(new object(), "", Key, Timeout));
            exception.ParamName.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void SendCommandThrows_ArgumentException_WhenQueueNameIsWhitespace()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.SendCommandAsync<string>(new object(), " ", Key, Timeout));
            exception.ParamName.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void SendCommandThrows_ArgumentNullException_WhenMessageIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() => _sut.SendCommandAsync<string>(null, QueueName, Key, Timeout));
            exception.ParamName.ShouldBe(MessageParamName);
        }

        [TestMethod]
        public void SendCommandThrows_ArgumentNullException_WhenKeyIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() => _sut.SendCommandAsync<string>(new object(), QueueName, null, Timeout));
            exception.ParamName.ShouldBe(KeyParamName);
        }

        [TestMethod]
        public void SendCommandThrows_ArgumentException_WhenKeyIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.SendCommandAsync<string>(new object(), QueueName, "", Timeout));
            exception.ParamName.ShouldBe(KeyParamName);
        }

        [TestMethod]
        public void SendCommandThrows_ArgumentException_WhenKeyIsWhitespace()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.SendCommandAsync<string>(new object(), QueueName, " ", Timeout));
            exception.ParamName.ShouldBe(KeyParamName);
        }

        [TestMethod]
        public void SendCommandThrows_ArgumentException_WhenKeyContainsWildCardStar()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.SendCommandAsync<string>(new object(), QueueName, "test.*.event", Timeout));
            exception.Message.ShouldBe(WildcardExceptionMessage);
        }

        [TestMethod]
        public void SendCommandThrows_ArgumentException_WhenKeyContainsWildCardHashtag()
        {
            var exception = Should.Throw<ArgumentException>(() => _sut.SendCommandAsync<string>(new object(), QueueName, "test.#.event", Timeout));
            exception.Message.ShouldBe(WildcardExceptionMessage);
        }

        private Expression<Func<EventMessage, bool>> CorrectEventMessage =>
            eventMessage =>
                eventMessage.RoutingKey == Key &&
                eventMessage.JsonMessage == JsonConvert.SerializeObject(Message);

        private Action<EventMessage> BasicPublishCallback =>
            receivedEvent =>
            {
                string receivedMessage = JsonConvert.DeserializeObject<string>(receivedEvent.JsonMessage);
                string invertedMessage = receivedMessage.Reverse().ToString();

                _eventReceivedCallback(new EventMessage
                {
                    CorrelationId = receivedEvent.CorrelationId,
                    JsonMessage = JsonConvert.SerializeObject(invertedMessage),
                    RoutingKey = receivedEvent.RoutingKey,
                    Type = receivedEvent.Type
                });
            };
    }
}
using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Remiworks.Core.Command;
using Remiworks.Core.Models;
using Shouldly;

namespace Remiworks.Core.Test.Command
{
    [TestClass]
    public class CommandPublisherTests
    {
        private const string Message = "testMessage";
        private const string Key = "testKey";
        private const string QueueName = "testQueue";

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
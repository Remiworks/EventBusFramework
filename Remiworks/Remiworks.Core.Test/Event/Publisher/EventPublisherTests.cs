using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Remiworks.Core.Event.Publisher;
using Remiworks.Core.Models;
using Remiworks.Core.Test.Stubs;

namespace Remiworks.Core.Test.Event.Publisher
{
    [TestClass]
    public class EventPublisherTests
    {
        private const string Topic = "Person.Created";
        
        private readonly Person _person = new Person {Name = "Jan Janssen"};
            
        private readonly Mock<IBusProvider> _busProviderMock = new Mock<IBusProvider>(MockBehavior.Strict);
        
        private EventPublisher _sut;

        [TestInitialize]
        public void Initialize()
        {
            _busProviderMock
                .Setup(b => b.BasicTopicBind(It.IsAny<string>(), It.IsAny<string>()));
            
            _busProviderMock
                .Setup(b => b.BasicPublish(It.IsAny<EventMessage>()));

            _busProviderMock
                .Setup(b => b.EnsureConnection());

            _sut = new EventPublisher(_busProviderMock.Object);
        }

        [TestMethod]
        public async Task SendEventAsyncCallsBusProvider_BasicPublish_WithJson()
        {
            var personJson = JsonConvert.SerializeObject(_person);
            
            await _sut.SendEventAsync(_person, Topic);
            
            _busProviderMock
                .Verify(b => b.BasicPublish(
                    It.Is<EventMessage>(m => m.JsonMessage == personJson)));
        }

        [TestMethod]
        public async Task SendEventAsyncCallsBusProvider_BasicPublish_WithTopic()
        {
            await _sut.SendEventAsync(_person, Topic);
            
            _busProviderMock
                .Verify(b => b.BasicPublish(
                    It.Is<EventMessage>(m => m.RoutingKey == Topic)));
        }

        [TestMethod]
        public async Task SendEventAsyncCallsBusProvider_BasicPublish_WithoutUnnecessaryProperties()
        {
            await _sut.SendEventAsync(_person, Topic);
            
            _busProviderMock
                .Verify(b => b.BasicPublish(
                    It.Is<EventMessage>(m => 
                        m.IsError == false &&
                        m.ReplyQueueName == null &&
                        m.Type == null &&
                        m.CorrelationId == null &&
                        m.Timestamp == null)));            
        }
    }
}
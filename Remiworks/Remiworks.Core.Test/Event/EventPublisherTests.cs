using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Remiworks.Core.Event;

namespace Remiworks.Core.Test.Event
{
    [TestClass]
    public class EventPublisherTests
    {
        private readonly Mock<IBusProvider> _busProviderMock = new Mock<IBusProvider>(MockBehavior.Strict);
        
        private EventPublisher _sut;

        [TestInitialize]
        public void Initialize()
        {
            _sut = new EventPublisher(_busProviderMock.Object);
        }

        [TestMethod]
        public void BindTopicsToQueueCallsBusProvider_CreateTopicsForQueue()
        {
            string queueName = "TestQueue";
            string[] topics = new string[] { "SomeTopic1", "SomeTopic2" };
            
            _busProviderMock
                .Setup(b => b.CreateTopicsForQueue(queueName, topics))
                .Verifiable();

            _sut.BindTopicsToQueue(queueName, topics).Wait();
            
            _busProviderMock.VerifyAll();
        }
    }
}
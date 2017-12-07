using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Remiworks.Core.Event;

namespace Remiworks.Core.Test.Event
{
    [TestClass]
    public class EventListenerTests
    {
        private readonly Mock<IBusProvider> _busProviderMock = new Mock<IBusProvider>();
        
        private EventListener _sut;
        
        [TestInitialize]
        public void TestInitialize()
        {
            _sut = new EventListener(_busProviderMock.Object);
        }

        [TestMethod]
        public void ListenToQueueCallsBusProvider_BasicConsume()
        {
            
        }

        [TestMethod]
        public void ListenToQueueCalls_EventReceived_WhenEventIsReceived()
        {
            
        }
    }
}
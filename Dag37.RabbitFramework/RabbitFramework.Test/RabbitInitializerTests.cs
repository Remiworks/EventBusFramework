using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitFramework.Test
{
    [TestClass]
    public class RabbitInitializerTests
    {
        private Mock<IBusProvider> _busProviderMock;

        [TestInitialize]
        public void Initialize()
        {
            _busProviderMock = new Mock<IBusProvider>(MockBehavior.Strict);
        }

        [TestMethod]
        public void InitializeCallsCreateConnection()
        {
            _busProviderMock.Setup(b => b.CreateConnection());
            RabbitInitializer target = new RabbitInitializer(_busProviderMock.Object);

            target.Initialize();

            _busProviderMock.Verify(b => b.CreateConnection(), Times.AtMostOnce);
        }

        [TestMethod]
        public void RegisterQueueToCallback()
        {
            _busProviderMock.Setup(m => m.BasicConsume("queue", It.IsAny<EventReceivedCallback>()));

            RabbitInitializer target = new RabbitInitializer(_busProviderMock.Object);

            target.RegisterQueueToCallback("queue");

            _busProviderMock.Verify(m => m.BasicConsume("queue", It.IsAny<EventReceivedCallback>()), Times.AtMostOnce);
        }

        [TestMethod]
        public void MyTestMethod()
        {

        }
    }
}

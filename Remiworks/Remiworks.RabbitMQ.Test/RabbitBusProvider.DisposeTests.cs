using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing.Impl;
using Remiworks.Core.Models;
using Shouldly;

namespace Remiworks.RabbitMQ.Test
{
    [TestClass]
    public class RabbitBusProviderDisposeTests
    {
        private readonly BusOptions _busOptions = new BusOptions { Port = 1234, UserName = "Jan", Password = "Secret" };
        private readonly Mock<ConnectionFactory> _connectionFactoryMock = new Mock<ConnectionFactory>();
        private readonly Mock<IConnection> _connectionMock = new Mock<IConnection>();
        private readonly Mock<IModel> _modelMock = new Mock<IModel>();

        private RabbitBusProvider _sut;

        [TestInitialize]
        public void SetUp()
        {
            _connectionFactoryMock
                .Setup(f => f.CreateConnection())
                .Returns(_connectionMock.Object);

            _connectionMock
                .Setup(c => c.CreateModel())
                .Returns(_modelMock.Object);
            
            _sut = new RabbitBusProvider(_busOptions, _connectionFactoryMock.Object);
        }

        [TestMethod]
        public void DisposeShouldDisposeConnectionAfterEnsureConnection()
        {
            _sut.EnsureConnection();

            _sut.Dispose();
            
            _connectionMock.Verify(c => c.Dispose(), Times.Once);
        }

        [TestMethod]
        public void DisposeShouldDisposeModelAfterEnsureConnection()
        {
            _sut.EnsureConnection();
            
            _sut.Dispose();
            
            _modelMock.Verify(m => m.Dispose(), Times.Once);
        }

        [TestMethod]
        public void DisposeShouldNotDisposeConnectionBeforeEnsureConnection()
        {
            _sut.Dispose();
            
            _connectionMock.Verify(c => c.Dispose(), Times.Never);
        }

        [TestMethod]
        public void DisposeShouldNotDisposeModelBeforeEnsureConnection()
        {
            _sut.Dispose();
            
            _modelMock.Verify(m => m.Dispose(), Times.Never);
        }
    }
}
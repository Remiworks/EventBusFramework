using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;

namespace RabbitFramework.Test
{
    [TestClass]
    public class RabbitBusProviderTests
    {
        private readonly Mock<BusOptions> _busOptionsMock = new Mock<BusOptions>();

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
    }
}
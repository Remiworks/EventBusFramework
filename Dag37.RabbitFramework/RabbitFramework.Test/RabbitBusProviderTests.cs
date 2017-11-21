using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;
using System;

namespace RabbitFramework.Test
{
    [TestClass]
    public class RabbitBusProviderTests
    {
        private readonly Mock<BusOptions> _busOptionsMock = new Mock<BusOptions>();

        private RabbitBusProvider _sut;

       
        [TestMethod]
        public void ConstructorSetsBusOptions()
        {
            _sut.BusOptions.ShouldBe(_busOptionsMock.Object);
        }
    }
}
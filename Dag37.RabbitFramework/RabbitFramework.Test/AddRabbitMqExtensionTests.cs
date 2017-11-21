using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitFramework.Test
{
    [TestClass]
    public class AddRabbitMqExtensionTests
    {
        Mock<IServiceProvider> serviceProviderMock;

        [TestInitialize]
        public void initialize()
        {
            serviceProviderMock = new Mock<IServiceProvider>();
        }

        [TestMethod]
        public void UseRabbitMqCallsGetServiceForIBusProvider()
        {
            //serviceProviderMock.Setup(s => s.Ge);
            //AddRabbitMqExtension.UseRabbitMq(serviceProviderMock.Object);
        }
    }
}

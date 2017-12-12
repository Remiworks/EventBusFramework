using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Remiworks.Attributes.Extensions;
using Shouldly;

namespace Remiworks.Attributes.Test.Extensions
{
    [TestClass]
    public class DependencyInjectionExtensionsTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock = new Mock<IServiceProvider>();
        
        [TestMethod]
        public void UseAttributesThrows_ArgumentNullException_WhenServiceProviderIsNull()
        {
            Should.Throw<ArgumentNullException>(() => 
                _serviceProviderMock.Object.UseAttributes());
        }
    }
}
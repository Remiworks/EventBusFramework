using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Remiworks.Attributes.Extensions;
using Shouldly;

namespace Remiworks.Attributes.Test.Extensions
{
    [TestClass]
    public class DependencyInjectionExtensionsTests
    {
        [TestMethod]
        public void UseAttributesThrows_ArgumentNullException_WhenServiceProviderIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() => 
                DependencyInjectionExtensions.UseAttributes(null));
            
            exception.ParamName.ShouldBe("serviceProvider");
        }
    }
}
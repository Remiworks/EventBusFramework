using Microsoft.VisualStudio.TestTools.UnitTesting;
using Remiworks.Core.Models;
using Shouldly;

namespace Remiworks.Core.Test
{
    [TestClass]
    public class BusOptionsTests
    {
        private BusOptions _sut;

        [TestInitialize]
        public void Initialize()
        {
            _sut = new BusOptions();
        }

        [TestMethod]
        public void HostnameCanBeSetAndGet()
        {
            string hostname = "Some value";

            _sut.Hostname = hostname;
            _sut.Hostname.ShouldBe(hostname);
        }

        [TestMethod]
        public void ExchangeNameCanBeSetAndGet()
        {
            string exchangeName = "Some value";

            _sut.ExchangeName = exchangeName;
            _sut.ExchangeName.ShouldBe(exchangeName);
        }

        [TestMethod]
        public void VirtualHostCanBeSetAndGet()
        {
            string virtualHost = "Some value";

            _sut.VirtualHost = virtualHost;
            _sut.VirtualHost.ShouldBe(virtualHost);
        }

        [TestMethod]
        public void PortCanBeSetAndGet()
        {
            int port = 23456;

            _sut.Port = port;
            _sut.Port.ShouldBe(port);
        }

        [TestMethod]
        public void UserNameCanBeSetAndGet()
        {
            string userName = "Some value";

            _sut.UserName = userName;
            _sut.UserName.ShouldBe(userName);
        }

        [TestMethod]
        public void PasswordCanBeSetAndGet()
        {
            string password = "Some value";

            _sut.Password = password;
            _sut.Password.ShouldBe(password);
        }

        [TestMethod]
        public void HostnameHasDefaultValue_localhost()
        {
            _sut.Hostname.ShouldBe("localhost");
        }

        [TestMethod]
        public void VirtualHostHasDefaultValue_ForwardSlash()
        {
            _sut.VirtualHost.ShouldBe("/");
        }
    }
}
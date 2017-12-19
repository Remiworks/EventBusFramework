using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Remiworks.Core.Command.Listener;
using Remiworks.Core.Command.Listener.Callbacks;
using Remiworks.Core.Models;

namespace Remiworks.Core.Test.Command.Listener
{
    [TestClass]
    public class CommandListenerTests
    {
        private const string QueueName = "someQueue";
        private const string Key = "some.key";
        
        private readonly Mock<IBusProvider> _busProviderMock = new Mock<IBusProvider>(MockBehavior.Strict);
        private readonly Mock<ICommandCallbackRegistry> _callbackRegistryMock = new Mock<ICommandCallbackRegistry>(MockBehavior.Strict);
        
        private CommandListener _sut;

        [TestInitialize]
        public void Initialize()
        {
            _callbackRegistryMock
                .Setup(c => c.AddCallbackForQueue(QueueName, Key, It.IsAny<Action<EventMessage>>()))
                .Verifiable();
            
            _sut = new CommandListener(_busProviderMock.Object, _callbackRegistryMock.Object);
        }

        [TestMethod]
        public async Task SetupCommandListenerCalls_CallbackRegistry()
        {
            await _sut.SetupCommandListenerAsync(QueueName, Key, new Mock<CommandReceivedCallback>().Object, new Mock<Type>().Object);
            
            _callbackRegistryMock.VerifyAll();
        }

        [TestMethod]
        public async Task SetupCommandListenerRegisters_WrappedCallback()
        {
            await _sut.SetupCommandListenerAsync(QueueName, Key, It.IsAny<CommandReceivedCallback>(), It.IsAny<Type>());
        }

        [TestMethod]
        public void SetupCommandListenerWraps_TargetInvocationException_InCommandPublisherException()
        {
            
        }

        [TestMethod]
        public void SetupCommandListenerWraps_AllOtherExceptions_InCommandPublisherException()
        {
            
        }
    }
}
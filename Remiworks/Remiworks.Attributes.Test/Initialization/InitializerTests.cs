using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Remiworks.Attributes.Initialization;
using Remiworks.Attributes.Test.Stubs;
using Remiworks.Core;
using Remiworks.Core.Command.Listener;
using Remiworks.Core.Event.Listener;

namespace Remiworks.Attributes.Test.Initialization
{
    [TestClass]
    public class InitializerTests
    {
        private const string Queue1 = "testQueue1";
        private const string Queue2 = "testQueue2";
        
        private const string Topic1 = "testTopic1";
        private const string Topic2 = "testTopic2";
        private const string Topic3 = "testTopic3";
        private const string Topic4 = "testTopic4";
        
        private readonly Mock<IBusProvider> _busProviderMock = new Mock<IBusProvider>();
        private readonly Mock<IEventListener> _eventListenerMock = new Mock<IEventListener>();
        private readonly Mock<ICommandListener> _commandListenerMock = new Mock<ICommandListener>();

        private Initializer _sut;

        [TestInitialize]
        public void Initialize()
        {
            var serviceProvider = new ServiceCollection()
                .AddTransient(s => _eventListenerMock.Object)
                .AddTransient(s => _commandListenerMock.Object)
                .AddTransient(s => _busProviderMock.Object)
                .BuildServiceProvider();
            
            SetupEventListener(Queue1, Topic1, typeof(Person));
            SetupEventListener(Queue1, Topic2, typeof(Person));
            SetupEventListener(Queue2, Topic3, typeof(Person));
            SetupEventListener(Queue2, Topic4, typeof(Person));

            _sut = new Initializer(serviceProvider);
        }

        [TestMethod]
        public void InitializeCallsEventListenerForEventAttributes()
        {
            _sut.Initialize(Assembly.GetAssembly(GetType()));
            
            _eventListenerMock.VerifyAll();
        }

        private void SetupEventListener(string queueName, string topic, Type parameterType)
        {
            _eventListenerMock
                .Setup(e => e.SetupQueueListenerAsync(
                    queueName, 
                    topic, 
                    It.IsAny<EventReceived>(), 
                    parameterType,
                    It.IsAny<string>()))
                .Verifiable();
        }
    }
}
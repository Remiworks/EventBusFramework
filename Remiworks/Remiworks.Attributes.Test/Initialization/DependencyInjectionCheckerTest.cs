using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Remiworks.Attributes.Initialization;
using Remiworks.Core;
using Remiworks.Core.Command;
using Remiworks.Core.Command.Publisher;
using Remiworks.Core.Event;
using Remiworks.Core.Event.Listener;
using Remiworks.Core.Event.Listener.Callbacks;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Remiworks.Attributes.Test.Initialization
{
    [TestClass]
    public class DependencyInjectionCheckerTest
    {
        private IServiceProvider _serviceProvider;

        private Mock<IEventListener> _eventListenerMock;
        private Mock<IEventCallbackRegistry> _eventCallbackMock;
        private Mock<ICommandPublisher> _commandPublisherMock;
        private Mock<IBusProvider> _busProviderMock;

        private List<Type> types;

        [TestInitialize]
        public void Initialize()
        {
            _eventListenerMock = new Mock<IEventListener>();
            _eventCallbackMock = new Mock<IEventCallbackRegistry>();
            _commandPublisherMock = new Mock<ICommandPublisher>();
            _busProviderMock = new Mock<IBusProvider>();

            _serviceProvider = new ServiceCollection()
                .AddTransient(m => _eventListenerMock.Object)
                .AddTransient(m => _busProviderMock.Object)
                .AddTransient(m => _eventCallbackMock.Object)
                .BuildServiceProvider();

            types = new List<Type>();
            types.Add(typeof(EventListener));

        }

        [TestMethod]
        public void CheckDependenciesReturnsEmptyStringArray()
        {
            var result = DependencyInjectionChecker.CheckDependencies(_serviceProvider, types).ToList();

            result.Count.ShouldBe(0);
        }

        [TestMethod]
        public void CheckDependenciesReturnsArrayWith1Error()
        {
            _serviceProvider = new ServiceCollection()
               .AddTransient(m => _eventListenerMock.Object)
               .BuildServiceProvider();
            
            var result = DependencyInjectionChecker.CheckDependencies(_serviceProvider, types).ToList();

            result.Count.ShouldBe(1);
            result[0].ShouldBe("Unable to resolve service for type 'Remiworks.Core.IBusProvider' while attempting to activate 'Remiworks.Core.Event.Listener.EventListener'.");
        }

        [TestMethod]
        public void CheckDepndenciesReturnsArrayWith2Errors()
        {
            types.Add(typeof(CommandPublisher));
            _serviceProvider = new ServiceCollection()
               .AddTransient(m => _eventListenerMock.Object)
               .AddTransient(m => _commandPublisherMock.Object)
               .BuildServiceProvider();
            
            var result = DependencyInjectionChecker.CheckDependencies(_serviceProvider, types).ToList();

            result.Count.ShouldBe(2);
            result[0].ShouldBe("Unable to resolve service for type 'Remiworks.Core.IBusProvider' while attempting to activate 'Remiworks.Core.Event.Listener.EventListener'.");
            result[1].ShouldBe("Unable to resolve service for type 'Remiworks.Core.IBusProvider' while attempting to activate 'Remiworks.Core.Command.Publisher.CommandPublisher'.");
        }
    }
}

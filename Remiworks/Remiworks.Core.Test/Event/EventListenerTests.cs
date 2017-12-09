using System;
using System.Collections;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Remiworks.Core.Event;
using Remiworks.Core.Models;
using Remiworks.Core.Test.Stubs;
using Shouldly;

namespace Remiworks.Core.Test.Event
{
    [TestClass]
    public class EventListenerTests
    {
        private const string QueueName = "testQueue";
        private const int Timeout = 2000;

        private static readonly Person Person = new Person {Name = "Jan"};
        private static readonly EventMessage EventMessage = new EventMessage
        {
            JsonMessage = JsonConvert.SerializeObject(Person),
            RoutingKey = "SomeKey"
        };
        
        private readonly Mock<IBusProvider> _busProviderMock = new Mock<IBusProvider>(MockBehavior.Strict);
        
        private EventListener _sut;
        
        [TestInitialize]
        public void TestInitialize()
        {
            _sut = new EventListener(_busProviderMock.Object);
        }

        [TestMethod]
        public void ListenToQueueCallsBusProvider_BasicConsume()
        {
            SetupBusProviderMockBasicConsume();
            
            _sut.SetupQueueListener(QueueName, new Mock<EventReceived<Person>>().Object).Wait();
            
            _busProviderMock.Verify(b => b.BasicConsume(QueueName, It.IsAny<EventReceivedCallback>()));
        }

        [TestMethod]
        public void ListenToQueueCalls_EventReceived_WhenEventIsReceived()
        {
            var waitHandle = new ManualResetEvent(false);
            
            EventReceivedCallback basicConsumeCallback = null;
            Person receivedPerson = null;
            string receivedTopic = null;
            
            SetupBusProviderMockBasicConsume((_, callback) => 
                basicConsumeCallback = callback);
            
            EventReceived<Person> eventListenerCallback = (person, topic) =>
            {
                receivedPerson = person;
                receivedTopic = topic;
                waitHandle.Set();
            };

            _sut.SetupQueueListener(QueueName, eventListenerCallback).Wait();
            basicConsumeCallback(EventMessage);

            waitHandle.WaitOne(Timeout).ShouldBeTrue();
            receivedPerson.Name.ShouldBe(Person.Name);
            receivedTopic.ShouldBe(EventMessage.RoutingKey);
        }

        private void SetupBusProviderMockBasicConsume(Action<string, EventReceivedCallback> mockCallback = null)
        {
            var setupMock = _busProviderMock.Setup(b => 
                b.BasicConsume(QueueName, It.IsAny<EventReceivedCallback>()));

            if (mockCallback != null)
            {
                setupMock.Callback(mockCallback);
            }
        }
    }
}
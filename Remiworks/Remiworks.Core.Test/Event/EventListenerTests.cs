using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Remiworks.Core.Event;
using Remiworks.Core.Models;
using Remiworks.Core.Test.Event.Stubs;
using Shouldly;

namespace Remiworks.Core.Test.Event
{
    [TestClass]
    public class EventListenerTests
    {
        private const string QueueName = "testQueue";
        private const string WildcardTopic = "foo.*.bar";
        private const string FullTopic = "foo.something.bar";
        private const int Timeout = 2000;

        private static readonly Person Person = new Person {Name = "Jan"};
        private static readonly EventMessage EventMessage = new EventMessage
        {
            JsonMessage = JsonConvert.SerializeObject(Person),
            RoutingKey = FullTopic
        };
        
        private readonly Mock<IBusProvider> _busProviderMock = new Mock<IBusProvider>(MockBehavior.Strict);
        
        private EventListener _sut;

        private ListenToQueueAndTopicStub<Person> _listenToQueueAndTopicStub;
        private ListenToQueueStub<Person> _listenToQueueStub;
        
        [TestInitialize]
        public void TestInitialize()
        {
            _sut = new EventListener(_busProviderMock.Object);
            
            _listenToQueueAndTopicStub = new ListenToQueueAndTopicStub<Person>();
            _listenToQueueStub = new ListenToQueueStub<Person>();
        }

        [TestMethod]
        public void ListenToQueueCallsBusProvider_BasicConsume()
        {
            SetupBusProviderMockBasicConsume(QueueName);
            
            _sut.SetupQueueListener(QueueName, new Mock<EventReceived<Person>>().Object).Wait();
            
            _busProviderMock.Verify(b => b.BasicConsume(QueueName, It.IsAny<EventReceivedCallback>()));
        }

        [TestMethod]
        public async void ListenToQueueCalls_EventReceived_WhenEventIsReceived()
        {
            EventReceivedCallback callbackFromBus = null;
            SetupBusProviderMockBasicConsume(
                QueueName,
                (_, callback) => callbackFromBus = callback);

            await _sut.SetupQueueListener(QueueName, _listenToQueueStub.EventListenerCallback());
            callbackFromBus.Invoke(EventMessage);

            _listenToQueueStub.WaitHandle.WaitOne(Timeout).ShouldBeTrue();
            _listenToQueueStub.ReceivedObject.Name.ShouldBe(Person.Name);
            _listenToQueueStub.ReceivedTopic.ShouldBe(EventMessage.RoutingKey);
        }

        [TestMethod]
        public async void ListenToQueueOverloadCalls_EventReceived_WhenEventIsReceivedForCorrectTopic()
        {
            EventReceivedCallback callbackFromBus = null;
            SetupBusProviderMockBasicConsume(
                QueueName,
                (_, callback) => callbackFromBus = callback);

            await _sut.SetupQueueListener(QueueName, WildcardTopic, _listenToQueueAndTopicStub.EventListenerCallback());
            callbackFromBus.Invoke(EventMessage);

            _listenToQueueAndTopicStub.WaitHandle.WaitOne(Timeout).ShouldBeTrue();
            _listenToQueueAndTopicStub.ReceivedObject.Name.ShouldBe(Person.Name);
        }

        [TestMethod]
        public async void ListenToQueueOverloadCalls_EventReceived_WhenEventIsReceivedForMultipleTopics()
        {
            EventReceivedCallback callbackFromBus = null;
            SetupBusProviderMockBasicConsume(
                QueueName, 
                (_, callback) => callbackFromBus = callback);

            var listenToQueueAndTopicStub2 = new ListenToQueueAndTopicStub<Person>();
            
            await _sut.SetupQueueListener(QueueName, WildcardTopic, _listenToQueueAndTopicStub.EventListenerCallback());
            await _sut.SetupQueueListener(QueueName, "foo.#", listenToQueueAndTopicStub2.EventListenerCallback());
            callbackFromBus.Invoke(EventMessage);

            _listenToQueueAndTopicStub.WaitHandle.WaitOne(Timeout).ShouldBeTrue();
            listenToQueueAndTopicStub2.WaitHandle.WaitOne(Timeout).ShouldBeTrue();
            
            _listenToQueueAndTopicStub.ReceivedObject.Name.ShouldBe(Person.Name);
            listenToQueueAndTopicStub2.ReceivedObject.Name.ShouldBe(Person.Name);
        }

        [TestMethod]
        public async void ListenToQueueOverloadCalls_EventReceived_WhenEventIsReceivedForMultipleQueues()
        {
            const string queueName2 = "otherQueue";
            
            EventReceivedCallback callbackFromBus1 = null;
            EventReceivedCallback callbackFromBus2 = null;
            
            SetupBusProviderMockBasicConsume(
                QueueName, 
                (_, callback) => callbackFromBus1 = callback);
            
            SetupBusProviderMockBasicConsume(
                queueName2, 
                (_, callback) => callbackFromBus2 = callback);
            
            var listenToQueueAndTopicStub2 = new ListenToQueueAndTopicStub<Person>();

            await _sut.SetupQueueListener(QueueName, WildcardTopic, _listenToQueueAndTopicStub.EventListenerCallback());
            await _sut.SetupQueueListener(queueName2, WildcardTopic, listenToQueueAndTopicStub2.EventListenerCallback());
            callbackFromBus1.Invoke(EventMessage);
            callbackFromBus2.Invoke(EventMessage);
            
            _listenToQueueAndTopicStub.WaitHandle.WaitOne(Timeout).ShouldBeTrue();
            listenToQueueAndTopicStub2.WaitHandle.WaitOne(Timeout).ShouldBeTrue();
            
            _listenToQueueAndTopicStub.ReceivedObject.Name.ShouldBe(Person.Name);
            listenToQueueAndTopicStub2.ReceivedObject.Name.ShouldBe(Person.Name);
        }

        private void SetupBusProviderMockBasicConsume(string queueName, Action<string, EventReceivedCallback> mockCallback = null)
        {
            var setupMock = _busProviderMock.Setup(b => 
                b.BasicConsume(queueName, It.IsAny<EventReceivedCallback>()));

            if (mockCallback != null)
            {
                setupMock.Callback(mockCallback);
            }
        }
    }
}
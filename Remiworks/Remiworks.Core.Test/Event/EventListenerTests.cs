using System;
using System.Collections;
using System.Threading.Tasks;
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
            _busProviderMock
                .Setup(b => b.CreateTopicsForQueue(It.IsAny<string>(), It.IsAny<string>()));
            
            _sut = new EventListener(_busProviderMock.Object);
            
            _listenToQueueAndTopicStub = new ListenToQueueAndTopicStub<Person>();
            _listenToQueueStub = new ListenToQueueStub<Person>();
        }

        [TestMethod]
        public async Task ListenToQueueCallsBusProvider_BasicConsume()
        {
            SetupBusProviderMockBasicConsume(QueueName);
            
            await _sut.SetupQueueListenerAsync(QueueName, new Mock<EventReceived<Person>>().Object);
            
            _busProviderMock.Verify(b => b.BasicConsume(QueueName, It.IsAny<EventReceivedCallback>()));
        }

        [TestMethod]
        public async Task ListenToQueueOverloadCallsBusProvider_BasicConsume()
        {
            SetupBusProviderMockBasicConsume(QueueName);
            
            await _sut.SetupQueueListenerAsync(QueueName, WildcardTopic, new Mock<EventReceivedForTopic>().Object, new Mock<Type>().Object);
            
            _busProviderMock.Verify(b => b.BasicConsume(QueueName, It.IsAny<EventReceivedCallback>()));
        }

        [TestMethod]
        public async Task ListenToQueueCallsBusProvider_CreateTopicsForQueue()
        {
            SetupBusProviderMockBasicConsume(QueueName);

            await _sut.SetupQueueListenerAsync(QueueName, WildcardTopic, new Mock<EventReceivedForTopic>().Object, new Mock<Type>().Object);
            
            _busProviderMock.Verify(b => b.CreateTopicsForQueue(QueueName, WildcardTopic));
        }

        [TestMethod]
        public async Task ListenToQueueCalls_EventReceived_WhenEventIsReceived()
        {
            EventReceivedCallback callbackFromBus = null;
            SetupBusProviderMockBasicConsume(
                QueueName,
                (_, callback) => callbackFromBus = callback);

            await _sut.SetupQueueListenerAsync(QueueName, _listenToQueueStub.GenericTypeEventListenerCallback());
            callbackFromBus.Invoke(EventMessage);

            _listenToQueueStub.WaitHandle.WaitOne(Timeout).ShouldBeTrue();
            _listenToQueueStub.ReceivedObject.Name.ShouldBe(Person.Name);
            _listenToQueueStub.ReceivedTopic.ShouldBe(EventMessage.RoutingKey);
        }

        [TestMethod]
        public async Task ListenToQueueOverloadCalls_EventReceived_WhenEventIsReceivedForCorrectTopic()
        {
            EventReceivedCallback callbackFromBus = null;
            SetupBusProviderMockBasicConsume(
                QueueName,
                (_, callback) => callbackFromBus = callback);

            await _sut.SetupQueueListenerAsync(QueueName, WildcardTopic, _listenToQueueAndTopicStub.GenericTypeEventListenerCallback());
            callbackFromBus.Invoke(EventMessage);

            _listenToQueueAndTopicStub.WaitHandle.WaitOne(Timeout).ShouldBeTrue();
            _listenToQueueAndTopicStub.ReceivedObject.Name.ShouldBe(Person.Name);
        }

        [TestMethod]
        public async Task ListenToQueueOverloadCalls_EventReceived_WhenEventIsReceivedForMultipleTopics()
        {
            EventReceivedCallback callbackFromBus = null;
            SetupBusProviderMockBasicConsume(
                QueueName, 
                (_, callback) => callbackFromBus = callback);

            var listenToQueueAndTopicStub2 = new ListenToQueueAndTopicStub<Person>();
            
            await _sut.SetupQueueListenerAsync(QueueName, WildcardTopic, _listenToQueueAndTopicStub.GenericTypeEventListenerCallback());
            await _sut.SetupQueueListenerAsync(QueueName, "foo.#", listenToQueueAndTopicStub2.GenericTypeEventListenerCallback());
            callbackFromBus.Invoke(EventMessage);

            _listenToQueueAndTopicStub.WaitHandle.WaitOne(Timeout).ShouldBeTrue();
            listenToQueueAndTopicStub2.WaitHandle.WaitOne(Timeout).ShouldBeTrue();
            
            _listenToQueueAndTopicStub.ReceivedObject.Name.ShouldBe(Person.Name);
            listenToQueueAndTopicStub2.ReceivedObject.Name.ShouldBe(Person.Name);
        }

        [TestMethod]
        public async Task ListenToQueueOverloadCalls_EventReceived_WhenEventIsReceivedForMultipleQueues()
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

            await _sut.SetupQueueListenerAsync(QueueName, WildcardTopic, _listenToQueueAndTopicStub.GenericTypeEventListenerCallback());
            await _sut.SetupQueueListenerAsync(queueName2, WildcardTopic, listenToQueueAndTopicStub2.GenericTypeEventListenerCallback());
            callbackFromBus1.Invoke(EventMessage);
            callbackFromBus2.Invoke(EventMessage);
            
            _listenToQueueAndTopicStub.WaitHandle.WaitOne(Timeout).ShouldBeTrue();
            listenToQueueAndTopicStub2.WaitHandle.WaitOne(Timeout).ShouldBeTrue();
            
            _listenToQueueAndTopicStub.ReceivedObject.Name.ShouldBe(Person.Name);
            listenToQueueAndTopicStub2.ReceivedObject.Name.ShouldBe(Person.Name);
        }

        [TestMethod]
        public async Task ListenToQueue_WithoutGenericTypeCalls_EventReceived_WhenEventIsReceived()
        {
            EventReceivedCallback callbackFromBus = null;
            SetupBusProviderMockBasicConsume(
                QueueName,
                (_, callback) => callbackFromBus = callback);

            await _sut.SetupQueueListenerAsync(QueueName, _listenToQueueStub.EventListenerCallback(), typeof(Person));
            callbackFromBus.Invoke(EventMessage);

            _listenToQueueStub.WaitHandle.WaitOne(Timeout).ShouldBeTrue();
            _listenToQueueStub.ReceivedObject.Name.ShouldBe(Person.Name);
            _listenToQueueStub.ReceivedTopic.ShouldBe(EventMessage.RoutingKey);
        }

        [TestMethod]
        public async Task ListenToQueueOverload_WithoutGenericTypeCalls_EventReceived_WhenEventIsReceivedForCorrectTopic()
        {
            EventReceivedCallback callbackFromBus = null;
            SetupBusProviderMockBasicConsume(
                QueueName,
                (_, callback) => callbackFromBus = callback);

            await _sut.SetupQueueListenerAsync(QueueName, WildcardTopic, _listenToQueueAndTopicStub.EventListenerCallback(), typeof(Person));
            callbackFromBus.Invoke(EventMessage);

            _listenToQueueAndTopicStub.WaitHandle.WaitOne(Timeout).ShouldBeTrue();
            _listenToQueueAndTopicStub.ReceivedObject.Name.ShouldBe(Person.Name);
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
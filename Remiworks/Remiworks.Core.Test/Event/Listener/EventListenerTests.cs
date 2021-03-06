﻿using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Remiworks.Core.Event;
using Remiworks.Core.Event.Listener;
using Remiworks.Core.Event.Listener.Callbacks;
using Remiworks.Core.Models;
using Remiworks.Core.Test.Event.Stubs;
using Remiworks.Core.Test.Stubs;
using Shouldly;

namespace Remiworks.Core.Test.Event.Listener
{
    [TestClass]
    public class EventListenerTests
    {
        private const string QueueNameParameter = "queueName";
        private const string CallbackParameter = "callback";
        private const string TypeParameter = "parameterType";
        
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
                .Setup(b => b.BasicTopicBind(It.IsAny<string>(), It.IsAny<string>()));
            _busProviderMock
                .Setup(b => b.EnsureConnection());

            _sut = new EventListener(_busProviderMock.Object, new EventCallbackRegistry(_busProviderMock.Object));
            
            _listenToQueueAndTopicStub = new ListenToQueueAndTopicStub<Person>();
            _listenToQueueStub = new ListenToQueueStub<Person>();
        }

        [TestMethod]
        public async Task ListenToQueueOverloadCallsBusProvider_BasicConsume()
        {
            SetupBusProviderMockBasicConsume(QueueName);
            
            await _sut.SetupQueueListenerAsync(QueueName, WildcardTopic, new Mock<EventReceived>().Object, new Mock<Type>().Object);
            
            _busProviderMock.Verify(b => b.BasicConsume(QueueName, It.IsAny<EventReceivedCallback>(), It.IsAny<bool>()));
        }

        [TestMethod]
        public async Task ListenToQueueCallsBusProvider_CreateTopicsForQueue()
        {
            SetupBusProviderMockBasicConsume(QueueName);

            await _sut.SetupQueueListenerAsync(QueueName, WildcardTopic, new Mock<EventReceived>().Object, new Mock<Type>().Object);
            
            _busProviderMock.Verify(b => b.BasicTopicBind(QueueName, WildcardTopic));
        }

        [TestMethod]
        public async Task ListenToQueueOverloadCalls_EventReceived_WhenEventIsReceivedForCorrectTopic()
        {
            EventReceivedCallback callbackFromBus = null;
            SetupBusProviderMockBasicConsume(
                QueueName,
                (queue, callback, ack) => callbackFromBus = callback);

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
                (queue, callback, ack) => callbackFromBus = callback);

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
                (queue, callback, ack) => callbackFromBus1 = callback);
            
            SetupBusProviderMockBasicConsume(
                queueName2, 
                (queue, callback, ack) => callbackFromBus2 = callback);
            
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
        public async Task ListenToQueueOverload_WithoutGenericTypeCalls_EventReceived_WhenEventIsReceivedForCorrectTopic()
        {
            EventReceivedCallback callbackFromBus = null;
            SetupBusProviderMockBasicConsume(
                QueueName,
                (queue, callback, ack) => callbackFromBus = callback);

            await _sut.SetupQueueListenerAsync(QueueName, WildcardTopic, _listenToQueueAndTopicStub.EventListenerCallback(), typeof(Person));
            callbackFromBus.Invoke(EventMessage);

            _listenToQueueAndTopicStub.WaitHandle.WaitOne(Timeout).ShouldBeTrue();
            _listenToQueueAndTopicStub.ReceivedObject.Name.ShouldBe(Person.Name);
        }

        private void SetupBusProviderMockBasicConsume(string queueName, Action<string, EventReceivedCallback, bool> mockCallback = null)
        {
            var setupMock = _busProviderMock.Setup(b => 
                b.BasicConsume(queueName, It.IsAny<EventReceivedCallback>(), It.IsAny<bool>()));

            if (mockCallback != null)
            {
                setupMock.Callback(mockCallback);
            }
        }
    }
}
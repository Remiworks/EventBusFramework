using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Remiworks.Core.Command.Listener;
using Remiworks.Core.Command.Listener.Callbacks;
using Remiworks.Core.Models;
using Remiworks.Core.Test.Stubs;
using Shouldly;

namespace Remiworks.Core.Test.Command.Listener
{
    [TestClass]
    public class CommandListenerTests
    {
        private const string QueueName = "someQueue";
        private const string ReplyQueueName = "someReplyQueue";
        private const string Key = "some.key";
        private const string ReplyKey = "some.key.Reply";
        private const ulong DeliveryTag = 12345;

        private static readonly Person Person = new Person { Name = "Jan Jannes" };

        private static readonly EventMessage IncomingEventMessage = new EventMessage
        {
            CorrelationId = new Guid("F9168C5E-CEB2-4faa-B6BF-329BF39FA1E4"),
            JsonMessage = JsonConvert.SerializeObject(Person),
            ReplyQueueName = ReplyQueueName,
            RoutingKey = Key,
            DeliveryTag = DeliveryTag
        };
        
        private readonly Mock<IBusProvider> _busProviderMock = new Mock<IBusProvider>(MockBehavior.Strict);
        private readonly Mock<ICommandCallbackRegistry> _callbackRegistryMock = new Mock<ICommandCallbackRegistry>(MockBehavior.Strict);
        
        private CommandListener _sut;

        [TestInitialize]
        public void Initialize()
        {
            _busProviderMock
                .Setup(b => b.BasicTopicBind(ReplyQueueName, It.Is(MatchingKey)));

            _busProviderMock
                .Setup(b => b.BasicPublish(It.Is(MatchingReplyEventMessage)));

            _busProviderMock
                .Setup(b => b.BasicAcknowledge(DeliveryTag, false));
            
            _sut = new CommandListener(_busProviderMock.Object, _callbackRegistryMock.Object);
        }

        [TestMethod]
        public async Task SetupCommandListenerCalls_CallbackRegistry()
        {
            SetupRegistryAddCallbackForQueue();
            
            await _sut.SetupCommandListenerAsync(QueueName, Key, new Mock<CommandReceivedCallback>().Object, new Mock<Type>().Object);
            
            _callbackRegistryMock.VerifyAll();
        }

        [TestMethod]
        public async Task SetupCommandListenerCalls_BasicTopicBind_WhenMessageComesIn()
        {
            Action<EventMessage> registeredAction = null;
            SetupRegistryAddCallbackForQueue((que, key, action) => registeredAction = action);
            
            await _sut.SetupCommandListenerAsync(QueueName, Key, new Mock<CommandReceivedCallback>().Object, typeof(Person));
            registeredAction(IncomingEventMessage);
            
            _busProviderMock.Verify(b => b.BasicTopicBind(ReplyQueueName, It.Is(MatchingKey)));
        }

        [TestMethod]
        public async Task SetupCommandListenerCalls_BasicPublish_WhenMessageComesIn()
        {
            Action<EventMessage> registeredAction = null;
            SetupRegistryAddCallbackForQueue((que, key, action) => registeredAction = action);
            
            await _sut.SetupCommandListenerAsync(QueueName, Key, new Mock<CommandReceivedCallback>().Object, typeof(Person));
            registeredAction(IncomingEventMessage);
            
            _busProviderMock.Verify(b => b.BasicPublish(It.Is(MatchingReplyEventMessage)));
        }

        [TestMethod]
        public async Task SetupCommandListenerCalls_BasicAcknowledge_WhenMessageComesIn()
        {
            Action<EventMessage> registeredAction = null;
            SetupRegistryAddCallbackForQueue((que, key, action) => registeredAction = action);
            
            await _sut.SetupCommandListenerAsync(QueueName, Key, new Mock<CommandReceivedCallback>().Object, typeof(Person));
            registeredAction(IncomingEventMessage);
            
            _busProviderMock.Verify(b => b.BasicAcknowledge(DeliveryTag, false));
        }

        [TestMethod]
        public async Task SetupCommandListenerRegisters_WrappedCallback()
        {
            Action<EventMessage> registeredAction = null;
            SetupRegistryAddCallbackForQueue((que, key, action) => registeredAction = action);
            
            var waitHandle = new ManualResetEvent(false);
            object receivedParameter = null;
            
            Task<object> Callback(object parameter)
            {
                waitHandle.Set();
                receivedParameter = parameter;
                
                return Task.FromResult<object>(null);
            }

            await _sut.SetupCommandListenerAsync(QueueName, Key, Callback, typeof(Person));
            registeredAction(IncomingEventMessage);

            waitHandle.WaitOne(2000).ShouldBeTrue();
            receivedParameter.ShouldBeOfType<Person>();
            ((Person)receivedParameter).Name.ShouldBe(Person.Name);
        }

        [TestMethod]
        public void SetupCommandListenerWraps_TargetInvocationException_InCommandPublisherException()
        {
            
        }

        [TestMethod]
        public void SetupCommandListenerWraps_AllOtherExceptions_InCommandPublisherException()
        {
            
        }

        private void SetupRegistryAddCallbackForQueue(Action<string, string, Action<EventMessage>> mockCallback = null)
        {
            var setupMock = _callbackRegistryMock.Setup(c => 
                c.AddCallbackForQueue(QueueName, Key, It.IsAny<Action<EventMessage>>()));

            if (mockCallback != null)
            {
                setupMock.Callback(mockCallback);
            }
        }

        private static Expression<Func<string[], bool>> MatchingKey =>
            keys =>
                keys.Length == 1 &&
                keys[0] == ReplyKey;

        private static Expression<Func<EventMessage, bool>> MatchingReplyEventMessage =>
            message =>
                message.RoutingKey == ReplyKey &&
                message.CorrelationId == IncomingEventMessage.CorrelationId;
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RabbitFramework.Test
{
    [TestClass]
    public class RabbitInitializerTests
    {
        private Mock<IBusProvider> _busProviderMock;

        [TestInitialize]
        public void Initialize()
        {
            _busProviderMock = new Mock<IBusProvider>(MockBehavior.Strict);
        }

        [TestMethod]
        public void InitializeCallsCreateConnection()
        {
            _busProviderMock.Setup(b => b.CreateConnection());
            _busProviderMock.Setup(b => b.CreateQueue(It.IsAny<string>()));
            _busProviderMock.Setup(b => b.BasicConsume(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EventReceivedCallback>()));
            RabbitInitializer target = new RabbitInitializer(_busProviderMock.Object);

            target.Initialize();

            _busProviderMock.Verify(b => b.CreateConnection(), Times.AtMostOnce);
        }

        [TestMethod]
        public void RegisterTopicCallsBasicConsume()
        {
            var methodInfo = typeof(TestModel).GetMethod("TestModelTestFunction");
            var queue = "testQueue";
            var topic = "testTopic";

            _busProviderMock.Setup(b => b.BasicConsume(queue, topic, It.IsAny<EventReceivedCallback>()));
            var target = new RabbitInitializer(_busProviderMock.Object);

            target.RegisterTopic(queue, topic, typeof(TestModel), methodInfo);

            _busProviderMock.Verify(b => b.BasicConsume(queue, topic, null), Times.AtMostOnce);
        }

        [TestMethod]
        public void CreateEventReceivedCallbackReturnsCallback()
        {
            var methodInfo = typeof(TestModel).GetMethod("TestModelTestFunction");
            var target = new RabbitInitializer(_busProviderMock.Object);

            var result = target.CreateEventReceivedCallback(typeof(TestModel), methodInfo);

            result.ShouldBeOfType<EventReceivedCallback>();
        }

        [TestMethod]
        public void CreateEventReceivedCallbackInvokeTestModelTestFunctionShouldNotThrowException()
        {
            var methodInfo = typeof(RabbitInitializerTestClass).GetMethod("TestModelTestFunction");
            var target = new RabbitInitializer(_busProviderMock.Object);

            var result = target.CreateEventReceivedCallback(typeof(RabbitInitializerTestClass), methodInfo);

            var json = @"{'Name': 'Bad Boys'}";

            result(new EventMessage() { JsonMessage = json });
        }

        [TestMethod]
        public void CreateEventReceivedCallbackInvokeTestModelTestFunction()
        {
            var methodInfo = typeof(RabbitInitializerTestClass).GetMethod("TestModelTestTwoFunctionThrowsArgumentException");
            var target = new RabbitInitializer(_busProviderMock.Object);

            var result = target.CreateEventReceivedCallback(typeof(RabbitInitializerTestClass), methodInfo);

            var json = @"{'Name': 'Bad Boys'}";

            Should.Throw<TargetInvocationException>(() => result(new EventMessage() { JsonMessage = json }));
        }

        [TestMethod]
        public void CreateQueueCallsCreateQueue()
        {
            var queue = "queue";
            _busProviderMock.Setup(b => b.CreateQueue(queue));
            RabbitInitializer target = new RabbitInitializer(_busProviderMock.Object);

            target.RegisterQueue(queue);

            _busProviderMock.Verify(b => b.CreateConnection(), Times.AtMostOnce);
        }
    }
}

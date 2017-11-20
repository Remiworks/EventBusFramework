using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
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
            _busProviderMock.Setup(b => b.CreateQueueWithTopics(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()));
            _busProviderMock.Setup(b => b.BasicConsume(It.IsAny<string>(), It.IsAny<EventReceivedCallback>()));
            RabbitInitializer target = new RabbitInitializer(_busProviderMock.Object);

            target.Initialize();

            _busProviderMock.Verify(b => b.CreateConnection(), Times.AtMostOnce);
        }

        [TestMethod]
        public void CreateEventReceivedCallbackReturnsCallback()
        {
            var methodInfo = typeof(TestModel).GetMethod("TestModelTestFunction");
            var target = new RabbitInitializer(_busProviderMock.Object);

            var result = target.CreateEventReceivedCallback(typeof(TestModel), null);

            result.ShouldBeOfType<EventReceivedCallback>();
        }

        [TestMethod]
        public void GetTopicWithMethodsReturnsDictionary()
        {
            var target = new RabbitInitializer(_busProviderMock.Object);

            var result = target.GetTopicsWithMethods(typeof(RabbitInitializerTestClass));

            result.Count.ShouldBe(2);
            result.Any(r => r.Key == "testTopic").ShouldBeTrue();
            result.Any(r => r.Key == "testTwoTopic").ShouldBeTrue();
        }

        [TestMethod]
        public void GetTopicMatchesRoutingKeyEqualsTopic()
        {
            var topics = new Dictionary<string, MethodInfo>();
            topics.Add("user.event.deleted", null);
            topics.Add("user.event.added", null);

            var routingKey = "user.event.added";

            var target = new RabbitInitializer(_busProviderMock.Object);

            var result = target.GetTopicMatches(routingKey, topics);

            result.Count.ShouldBe(1);
            result.Any(r => r.Key == routingKey).ShouldBeTrue();
        }

        [TestMethod]
        public void GetTopicMatchesRoutingKeyStarTopic()
        {
            var topics = new Dictionary<string, MethodInfo>();
            topics.Add("user.*.deleted", null);
            topics.Add("user.event.added", null);

            var routingKey = "user.event.deleted";

            var target = new RabbitInitializer(_busProviderMock.Object);

            var result = target.GetTopicMatches(routingKey, topics);

            result.Count.ShouldBe(1);
            result.Any(r => r.Key == "user.*.deleted").ShouldBeTrue();
        }

        [TestMethod]
        public void GetTopicMatchesRoutingKeyHashTagTopic()
        {
            var topics = new Dictionary<string, MethodInfo>();
            topics.Add("user.#.deleted", null);
            topics.Add("user.event.added", null);

            var routingKey = "user.event.test.deleted";

            var target = new RabbitInitializer(_busProviderMock.Object);

            var result = target.GetTopicMatches(routingKey, topics);

            result.Count.ShouldBe(1);
            result.Any(r => r.Key == "user.#.deleted").ShouldBeTrue();
        }

        [TestMethod]
        public void GetTopicMatchesRoutingKeyHashTagAndStarTopic()
        {
            var topics = new Dictionary<string, MethodInfo>();
            topics.Add("user.#", null);
            topics.Add("user.event.added", null);

            var routingKey = "user..";

            var target = new RabbitInitializer(_busProviderMock.Object);

            var result = target.GetTopicMatches(routingKey, topics);

            result.Count.ShouldBe(0);
            result.Any(r => r.Key == "user.#").ShouldBeFalse();
        }

        [TestMethod]
        public void GetTopicMatchesRoutingKeyStarDotsTopic()
        {
            var topics = new Dictionary<string, MethodInfo>();
            topics.Add("user.*", null);
            topics.Add("user.event.added", null);

            var routingKey = "user..";

            var target = new RabbitInitializer(_busProviderMock.Object);

            var result = target.GetTopicMatches(routingKey, topics);

            result.Count.ShouldBe(0);
            result.Any(r => r.Key == "user.*").ShouldBeFalse();
        }

        [TestMethod]
        public void GetTopicMatchesRoutingKeyMoreThanTwoWordsTopic()
        {

            var topics = new Dictionary<string, MethodInfo>();
            topics.Add("user.#.deleted", null);
            topics.Add("user.event.added", null);

            var routingKey = "user.event.test.iets.deleted";

            var target = new RabbitInitializer(_busProviderMock.Object);

            var result = target.GetTopicMatches(routingKey, topics);

            result.Count.ShouldBe(1);
            result.Any(r => r.Key == "user.#.deleted").ShouldBeTrue();
        }

        [TestMethod]
        public void GetTopicMatchesRoutingKeyStartwithRandomtextTopic()
        {

            var topics = new Dictionary<string, MethodInfo>();
            topics.Add("user.#.deleted", null);
            topics.Add("user.event.added", null);

            var routingKey = "blablabla.user.event.test.iets.deleted";

            var target = new RabbitInitializer(_busProviderMock.Object);

            var result = target.GetTopicMatches(routingKey, topics);

            result.Count.ShouldBe(0);
            //result.Any(r => r.Key == "user.#.deleted").ShouldBeTrue();
        }

        [TestMethod]
        public void GetTopicMatchesRoutingKeyEndWithRandomtextTopic()
        {

            var topics = new Dictionary<string, MethodInfo>();
            topics.Add("user.#.deleted", null);
            topics.Add("user.event.added", null);

            var routingKey = "user.event.test.iets.deleted.blablabla";

            var target = new RabbitInitializer(_busProviderMock.Object);

            var result = target.GetTopicMatches(routingKey, topics);

            result.Count.ShouldBe(0);
            //result.Any(r => r.Key == "user.#.deleted").ShouldBeTrue();
        }
    }
}

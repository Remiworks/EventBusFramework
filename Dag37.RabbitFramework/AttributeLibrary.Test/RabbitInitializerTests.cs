using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitFramework.Contracts;
using RabbitFramework.Test;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AttributeLibrary.Test
{
    [TestClass]
    public class RabbitInitializerTests
    {
        private readonly Mock<IBusProvider> _busProviderMock = new Mock<IBusProvider>(MockBehavior.Strict);

        private RabbitInitializer _sut;

        [TestInitialize]
        public void Initialize()
        {
            _sut = new RabbitInitializer(_busProviderMock.Object, null);
        }

        [TestMethod]
        public void InitializeCallsCreateConnection()
        {
            _busProviderMock.Setup(b => b.CreateConnection());
            _busProviderMock.Setup(b => b.CreateQueueWithTopics(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()));
            _busProviderMock.Setup(b => b.BasicConsume(It.IsAny<string>(), It.IsAny<EventReceivedCallback>()));
            RabbitInitializer target = new RabbitInitializer(_busProviderMock.Object, null);

            target.Initialize(Assembly.GetCallingAssembly());

            _busProviderMock.Verify(b => b.CreateConnection(), Times.Once);
        }

        [TestMethod]
        public void CreateEventReceivedCallbackReturnsCallback()
        {
            var result = _sut.CreateEventReceivedCallback(typeof(TestModel), null);

            result.ShouldNotBeNull();
        }

        [TestMethod]
        public void GetTopicWithMethodsReturnsDictionary()
        {
            var result = _sut.GetTopicsWithMethods(typeof(RabbitInitializerTestClass));

            result.Count.ShouldBe(2);
            result.Any(r => r.Key == "testTopic").ShouldBeTrue();
            result.Any(r => r.Key == "testTwoTopic").ShouldBeTrue();
        }

        [TestMethod]
        public void GetTopicMatchesRoutingKeyEqualsTopic()
        {
            var topics = new Dictionary<string, MethodInfo>
            {
                { "user.event.deleted", null },
                { "user.event.added", null }
            };

            var routingKey = "user.event.added";

            var result = _sut.GetTopicMatches(routingKey, topics);

            result.Count.ShouldBe(1);
            result.Any(r => r.Key == routingKey).ShouldBeTrue();
        }

        [TestMethod]
        public void GetTopicMatchesRoutingKeyStarTopic()
        {
            var topics = new Dictionary<string, MethodInfo>
            {
                { "user.*.deleted", null },
                { "user.event.added", null }
            };

            var routingKey = "user.event.deleted";

            var result = _sut.GetTopicMatches(routingKey, topics);

            result.Count.ShouldBe(1);
            result.Any(r => r.Key == "user.*.deleted").ShouldBeTrue();
        }

        [TestMethod]
        public void GetTopicMatchesRoutingKeyHashTagTopic()
        {
            var topics = new Dictionary<string, MethodInfo>
            {
                { "user.#.deleted", null },
                { "user.event.added", null }
            };

            var routingKey = "user.event.test.deleted";

            var result = _sut.GetTopicMatches(routingKey, topics);

            result.Count.ShouldBe(1);
            result.Any(r => r.Key == "user.#.deleted").ShouldBeTrue();
        }

        [TestMethod]
        public void GetTopicMatchesRoutingKeyHashTagAndStarTopic()
        {
            var topics = new Dictionary<string, MethodInfo>
            {
                { "user.#", null },
                { "user.event.added", null }
            };

            var routingKey = "user..";

            var result = _sut.GetTopicMatches(routingKey, topics);

            result.Count.ShouldBe(0);
            result.Any(r => r.Key == "user.#").ShouldBeFalse();
        }

        [TestMethod]
        public void GetTopicMatchesRoutingKeyStarDotsTopic()
        {
            var topics = new Dictionary<string, MethodInfo>
            {
                { "user.*", null },
                { "user.event.added", null }
            };

            var routingKey = "user..";

            var result = _sut.GetTopicMatches(routingKey, topics);

            result.Count.ShouldBe(0);
            result.Any(r => r.Key == "user.*").ShouldBeFalse();
        }

        [TestMethod]
        public void GetTopicMatchesRoutingKeyMoreThanTwoWordsTopic()
        {
            var topics = new Dictionary<string, MethodInfo>
            {
                { "user.#.deleted", null },
                { "user.event.added", null }
            };

            var routingKey = "user.event.test.iets.deleted";

            var result = _sut.GetTopicMatches(routingKey, topics);

            result.Count.ShouldBe(1);
            result.Any(r => r.Key == "user.#.deleted").ShouldBeTrue();
        }

        [TestMethod]
        public void GetTopicMatchesRoutingKeyStartwithRandomtextTopic()
        {
            var topics = new Dictionary<string, MethodInfo>
            {
                { "user.#.deleted", null },
                { "user.event.added", null }
            };

            var routingKey = "blablabla.user.event.test.iets.deleted";

            var result = _sut.GetTopicMatches(routingKey, topics);

            result.Count.ShouldBe(0);
        }

        [TestMethod]
        public void GetTopicMatchesRoutingKeyEndWithRandomtextTopic()
        {
            var topics = new Dictionary<string, MethodInfo>
            {
                { "user.#.deleted", null },
                { "user.event.added", null }
            };

            var routingKey = "user.event.test.iets.deleted.blablabla";

            var result = _sut.GetTopicMatches(routingKey, topics);

            result.Count.ShouldBe(0);
        }
    }
}
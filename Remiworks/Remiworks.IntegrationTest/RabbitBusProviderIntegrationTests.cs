using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Schema;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Remiworks.Core;
using Remiworks.Core.Models;
using Remiworks.RabbitMQ;
using Shouldly;

namespace Remiworks.IntegrationTest
{
    [TestClass]
    public class RabbitBusProviderIntegrationTests : RabbitIntegrationTest
    {
        [TestMethod]
        public void EventCanBeReceived()
        {
            using (var sut = new RabbitBusProvider(BusOptions))
            {
                sut.EnsureConnection();
                var queue = GetUniqueQueue();
                var topic = GetUniqueTopic();
                const string jsonMessage = "Something";

                EventMessage passedMessage = null;
                var waitHandle = new ManualResetEvent(false);
                EventReceivedCallback eventReceivedCallback = (message) =>
                {
                    passedMessage = message;
                    waitHandle.Set();
                };

                sut.BasicTopicBind(queue, topic);
                sut.BasicConsume(queue, eventReceivedCallback);

                SendRabbitEventToExchange(topic, jsonMessage);

                waitHandle.WaitOne(2000).ShouldBeTrue();
                passedMessage.ShouldNotBeNull();
                passedMessage.JsonMessage.ShouldBe(jsonMessage);
            }
        }

        [TestMethod]
        public void EventCanBePublished()
        {
            using (var sut = new RabbitBusProvider(BusOptions))
            {
                sut.EnsureConnection();
                var queue = GetUniqueQueue();
                var topic = GetUniqueTopic();

                var waitHandle = new ManualResetEvent(false);
                BasicDeliverEventArgs passedArgs = null;

                var message = new EventMessage
                {
                    JsonMessage = "Something",
                    RoutingKey = topic,
                    Type = TopicType
                };

                ConsumeRabbitEvent(queue, (sender, args) =>
                {
                    waitHandle.Set();
                    passedArgs = args;
                });

                sut.BasicTopicBind(queue, topic);
                sut.BasicPublish(message);

                waitHandle.WaitOne(2000).ShouldBeTrue();
                var receivedMessage = Encoding.UTF8.GetString(passedArgs.Body);
                receivedMessage.ShouldBe(message.JsonMessage);
                passedArgs.RoutingKey.ShouldBe(message.RoutingKey);
            }
        }

        [TestMethod]
        public void EventCanBePublishedAndReceived()
        {
            using (var sut = new RabbitBusProvider(BusOptions))
            {
                sut.EnsureConnection();
                var queue = GetUniqueQueue();
                var topic = GetUniqueTopic();

                EventMessage receivedMessage = null;
                var waitHandle = new ManualResetEvent(false);
                EventReceivedCallback eventReceivedCallback = (message) =>
                {
                    receivedMessage = message;
                    waitHandle.Set();
                };

                var sentMessage = new EventMessage
                {
                    JsonMessage = "Something",
                    RoutingKey = topic,
                    Type = TopicType
                };

                sut.BasicTopicBind(queue, topic);
                sut.BasicConsume(queue, eventReceivedCallback);
                sut.BasicPublish(sentMessage);

                waitHandle.WaitOne(2000).ShouldBeTrue();
                receivedMessage.JsonMessage.ShouldBe(sentMessage.JsonMessage);
            }
        }
    }
}

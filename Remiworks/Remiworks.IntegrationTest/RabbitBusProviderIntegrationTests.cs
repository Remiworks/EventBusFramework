//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using RabbitMQ.Client.Events;
//using Remiworks.Core;
//using Remiworks.Core.Models;
//using Remiworks.RabbitMQ;
//using Shouldly;
//
//namespace Remiworks.IntegrationTest
//{
//    [TestClass]
//    public class RabbitBusProviderIntegrationTests : RabbitIntegrationTest
//    {
//        [TestMethod]
//        public void EventCanBeReceived()
//        {
//            using (var sut = new RabbitBusProvider(BusOptions))
//            {
//                string queue = GetUniqueQueue();
//                string topic = GetUniqueTopic();
//                string jsonMessage = "Something";
//
//                EventMessage passedMessage = null;
//                ManualResetEvent waitHandle = new ManualResetEvent(false);
//                EventReceivedCallback eventReceivedCallback = (message) =>
//                {
//                    passedMessage = message;
//                    waitHandle.Set();
//                };
//
//                sut.CreateConnection();
//                sut.CreateTopicsForQueue(queue, topic);
//                sut.BasicConsume(queue, eventReceivedCallback);
//
//                SendRabbitEventToExchange(topic, jsonMessage);
//
//                waitHandle.WaitOne(2000).ShouldBeTrue();
//                passedMessage.ShouldNotBeNull();
//                passedMessage.JsonMessage.ShouldBe(jsonMessage);
//            }
//        }
//
//        [TestMethod]
//        public void EventCanBePublished()
//        {
//            using (var sut = new RabbitBusProvider(BusOptions))
//            {
//                string queue = GetUniqueQueue();
//                string topic = GetUniqueTopic();
//
//                ManualResetEvent waitHandle = new ManualResetEvent(false);
//                BasicDeliverEventArgs passedArgs = null;
//
//                EventMessage message = new EventMessage
//                {
//                    JsonMessage = "Something",
//                    RoutingKey = topic,
//                    Type = TopicType
//                };
//
//                ConsumeRabbitEvent(queue, (sender, args) =>
//                {
//                    waitHandle.Set();
//                    passedArgs = args;
//                });
//
//                sut.CreateConnection();
//                sut.CreateTopicsForQueue(queue, topic);
//                sut.BasicPublish(message);
//
//                waitHandle.WaitOne(2000).ShouldBeTrue();
//                string receivedMessage = Encoding.UTF8.GetString(passedArgs.Body);
//                receivedMessage.ShouldBe(message.JsonMessage);
//                passedArgs.RoutingKey.ShouldBe(message.RoutingKey);
//            }
//        }
//
//        [TestMethod]
//        public void EventCanBePublishedAndReceived()
//        {
//            using (var sut = new RabbitBusProvider(BusOptions))
//            {
//                string queue = GetUniqueQueue();
//                string topic = GetUniqueTopic();
//
//                EventMessage receivedMessage = null;
//                ManualResetEvent waitHandle = new ManualResetEvent(false);
//                EventReceivedCallback eventReceivedCallback = (message) =>
//                {
//                    receivedMessage = message;
//                    waitHandle.Set();
//                };
//
//                EventMessage sentMessage = new EventMessage
//                {
//                    JsonMessage = "Something",
//                    RoutingKey = topic,
//                    Type = TopicType
//                };
//
//                sut.CreateConnection();
//                sut.CreateTopicsForQueue(queue, topic);
//                sut.BasicConsume(queue, eventReceivedCallback);
//                sut.BasicPublish(sentMessage);
//
//                waitHandle.WaitOne(2000).ShouldBeTrue();
//                receivedMessage.JsonMessage.ShouldBe(sentMessage.JsonMessage);
//            }
//        }
//    }
//}

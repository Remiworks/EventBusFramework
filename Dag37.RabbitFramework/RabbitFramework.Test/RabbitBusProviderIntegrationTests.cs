using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RabbitFramework.Test
{
    [TestClass]
    public class RabbitBusProviderIntegrationTests
    {
        private const string Host = "localhost";
        private const int Port = 5672;
        private const string UserName = "guest";
        private const string Password = "guest";
        private const string ExchangeName = "testExchange";
        private const string QueueName = "testQueue";
        private const string TopicName = "testTopic";
        private const string TopicType = "topic";

        private RabbitBusProvider _sut;

        [ClassInitialize]
        public static void ClassInitialize()
        {

        }

        [ClassCleanup]
        public static void ClassCleanup()
        {

        }

        [TestInitialize]
        public void Initialize()
        {
            BusOptions busOptions = new BusOptions()
            {
                Hostname = Host,
                Port = Port,
                UserName = UserName,
                Password = Password,
                ExchangeName = ExchangeName
            };

            _sut = new RabbitBusProvider(busOptions);
        }

        [TestMethod]
        public void EventReceivedCallbackIsInvokedWithEvent()
        {
            string jsonMessage = "Something";

            EventMessage passedMessage = null;
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            EventReceivedCallback eventReceivedCallback = (message) =>
            {
                passedMessage = message;
                waitHandle.Set();
            };

            _sut.CreateConnection();
            _sut.CreateQueueWithTopics(QueueName, new List<string> { TopicName });
            _sut.BasicConsume(QueueName, eventReceivedCallback);

            SendRabbitEvent(jsonMessage);

            waitHandle.WaitOne(2000).ShouldBeTrue();
            passedMessage.ShouldNotBeNull();
            passedMessage.JsonMessage.ShouldBe(jsonMessage);
        }

        [TestMethod]
        public void EventIsSentAndCanBeReceived()
        {
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            BasicDeliverEventArgs passedArgs = null;

            EventMessage message = new EventMessage
            {
                JsonMessage = "Something",
                RoutingKey = TopicName,
                Type = TopicType
            };

            ConsumeRabbitEvent((sender, args) =>
            {
                waitHandle.Set();
                passedArgs = args;
            });
            
            waitHandle.WaitOne(2000).ShouldBeTrue();
            string receivedMessage = Encoding.UTF8.GetString(passedArgs.Body);
            receivedMessage.ShouldBe(message.JsonMessage);
            passedArgs.RoutingKey.ShouldBe(message.RoutingKey);
            passedArgs.BasicProperties.Type.ShouldBe(message.Type);
        }

        private void SendRabbitEvent(string json)
        {
            var factory = CreateConnectionFactory();

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(ExchangeName, TopicType);
                channel.BasicPublish(exchange: ExchangeName,
                                     routingKey: TopicName,
                                     basicProperties: null,
                                     body: Encoding.UTF8.GetBytes(json));
            }
        }

        private void ConsumeRabbitEvent(EventHandler<BasicDeliverEventArgs> callback)
        {
            var factory = CreateConnectionFactory();

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(QueueName);
                channel.QueueBind(QueueName, ExchangeName, TopicName);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += callback;
            }
        }

        private IConnectionFactory CreateConnectionFactory()
        {
            return new ConnectionFactory()
            {
                HostName = Host,
                Port = Port,
                UserName = UserName,
                Password = Password
            };
        }
    }
}
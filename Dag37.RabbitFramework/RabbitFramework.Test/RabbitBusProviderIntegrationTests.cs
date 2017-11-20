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
        private const string TopicType = "topic";

        private readonly BusOptions BusOptions = new BusOptions()
        {
            Hostname = Host,
            Port = Port,
            UserName = UserName,
            Password = Password,
            ExchangeName = ExchangeName
        };

        private IConnection _connection;
        private IModel _channel;

        [TestInitialize]
        public void Initialize()
        {
            OpenRabbitConnection();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _connection.Dispose();
            _channel.Dispose();
        }

        [TestMethod]
        public void EventReceivedCallbackIsInvokedWithEvent()
        {
            using (var sut = new RabbitBusProvider(BusOptions))
            {
                string queue = UniqueQueue();
                string topic = UniqueTopic();
                string jsonMessage = "Something";

                EventMessage passedMessage = null;
                ManualResetEvent waitHandle = new ManualResetEvent(false);
                EventReceivedCallback eventReceivedCallback = (message) =>
                {
                    passedMessage = message;
                    waitHandle.Set();
                };

                sut.CreateConnection();
                sut.CreateQueueWithTopics(queue, new List<string> { topic });
                sut.BasicConsume(queue, eventReceivedCallback);

                SendRabbitEvent(topic, jsonMessage);

                waitHandle.WaitOne(2000).ShouldBeTrue();
                passedMessage.ShouldNotBeNull();
                passedMessage.JsonMessage.ShouldBe(jsonMessage);
            }
        }


        [TestMethod]
        public void EventIsSentAndCanBeReceived()
        {
            using (var sut = new RabbitBusProvider(BusOptions))
            {
                string queue = UniqueQueue();
                string topic = UniqueTopic();

                ManualResetEvent waitHandle = new ManualResetEvent(false);
                BasicDeliverEventArgs passedArgs = null;

                EventMessage message = new EventMessage
                {
                    JsonMessage = "Something",
                    RoutingKey = topic,
                    Type = TopicType
                };

                ConsumeRabbitEvent(queue, topic, (sender, args) =>
                {
                    waitHandle.Set();
                    passedArgs = args;
                });

                sut.CreateConnection();
                sut.CreateQueueWithTopics(queue, new List<string> { topic });
                sut.BasicPublish(message);

                waitHandle.WaitOne(2000).ShouldBeTrue();
                string receivedMessage = Encoding.UTF8.GetString(passedArgs.Body);
                receivedMessage.ShouldBe(message.JsonMessage);
                passedArgs.RoutingKey.ShouldBe(message.RoutingKey);
            }
        }

        private string UniqueQueue()
        {
            return $"TestQueue-{Guid.NewGuid()}";
        }

        private string UniqueTopic()
        {
            return $"TestTopic.{Guid.NewGuid()}";
        }

        private void OpenRabbitConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = Host,
                Port = Port,
                UserName = UserName,
                Password = Password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(ExchangeName, TopicType);
        }

        private void SendRabbitEvent(string topic, string json)
        {
            _channel.BasicPublish(exchange: ExchangeName,
                                 routingKey: topic,
                                 basicProperties: null,
                                 body: Encoding.UTF8.GetBytes(json));
        }

        private void ConsumeRabbitEvent(string queue, string topic, EventHandler<BasicDeliverEventArgs> callback)
        {
            _channel.QueueDeclare(queue: queue, exclusive: false);
            _channel.QueueBind(queue, ExchangeName, topic);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += callback;

            _channel.BasicConsume(queue, true, consumer);
        }
    }
}
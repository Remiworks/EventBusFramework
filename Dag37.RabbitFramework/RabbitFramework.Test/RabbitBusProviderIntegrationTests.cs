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
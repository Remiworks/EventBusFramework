using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Remiworks.Core.Models;
using Remiworks.RabbitMQ;

namespace Remiworks.IntegrationTest
{
    public abstract class RabbitIntegrationTest
    {
        protected const string Host = "localhost";
        protected const int Port = 5672;
        protected const string UserName = "guest";
        protected const string Password = "guest";
        protected const string ExchangeName = "testExchange";
        protected const string TopicType = "topic";

        protected static readonly BusOptions BusOptions = new BusOptions()
        {
            Hostname = Host,
            Port = Port,
            UserName = UserName,
            Password = Password,
            ExchangeName = ExchangeName
        };

        protected IConnection _connection;
        protected IModel _channel;
        
        [TestInitialize]
        public void BaseInitialize()
        {
            try
            {
                OpenRabbitConnection();
            }
            catch (BrokerUnreachableException)
            {
                Assert.Fail(
                    $"RabbitMQ not reachable. Make sure that rabbitMq is started and running on {BusOptions.Hostname}:{BusOptions.Port}.");
            }
            catch (Exception e)
            {
                Assert.Fail(
                    "An unexpected exception occured while opening a connection " +
                    $"to RabbitMQ on {BusOptions.Hostname}:{BusOptions.Port}.\nException: {e}");
            }
        }

        [TestCleanup]
        public void BaseCleanup()
        {
            _connection?.Dispose();
            _channel?.Dispose();
        }

        protected static string GetUniqueQueue()
        {
            return $"TestQueue-{Guid.NewGuid()}";
        }

        protected static string GetUniqueTopic()
        {
            return $"TestTopic.{Guid.NewGuid()}";
        }

        protected void SendRabbitEventToQueue(string queue, string correlationId, string json)
        {
            var properties = _channel.CreateBasicProperties();
            properties.CorrelationId = correlationId;

            _channel.BasicPublish(
                exchange: "",
                routingKey: queue,
                basicProperties: properties,
                body: Encoding.UTF8.GetBytes(json));
        }

        protected void SendRabbitEventToExchange(string topic, string json)
        {
            _channel.BasicPublish(exchange: ExchangeName,
                                 routingKey: topic,
                                 basicProperties: null,
                                 body: Encoding.UTF8.GetBytes(json));
        }

        protected void ConsumeRabbitEvent(string queue, EventHandler<BasicDeliverEventArgs> callback)
        {
            _channel.QueueDeclare(queue: queue, exclusive: false, autoDelete: false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += callback;

            _channel.BasicConsume(queue, true, consumer);
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
    }
}
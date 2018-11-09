using System;
using System.Linq;
using System.Text;
using EnsureThat;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Remiworks.Core;
using Remiworks.Core.Models;

namespace Remiworks.RabbitMQ
{
    public class RabbitBusProvider : IBusProvider
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;

        public BusOptions BusOptions { get; }

        public RabbitBusProvider(BusOptions busOptions) : this(busOptions, new ConnectionFactory())
        {
        }

        public RabbitBusProvider(BusOptions busOptions, ConnectionFactory connectionFactory)
        {
            EnsureArg.IsNotNull(busOptions, nameof(busOptions));

            _connectionFactory = connectionFactory;
            BusOptions = busOptions;
            
            ProcessBusOptions();
        }

        private void ProcessBusOptions()
        {
            _connectionFactory.HostName = BusOptions.Hostname;
            _connectionFactory.VirtualHost = BusOptions.VirtualHost;
            
            if (BusOptions.Port != null) _connectionFactory.Port = BusOptions.Port.Value;
            if (BusOptions.UserName != null) _connectionFactory.UserName = BusOptions.UserName;
            if (BusOptions.Password != null) _connectionFactory.Password = BusOptions.Password;
        }

        public void EnsureConnection()
        {
            //TODO unit tests
            if (_connection != null && _connection.IsOpen)
                return;

            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void BasicConsume(string queueName, EventReceivedCallback callback, bool autoAcknowledge = true)
        {
            EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));
            EnsureArg.IsNotNull(callback, nameof(callback));

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (sender, args) => HandleReceivedEvent(args, callback);

            QueueDeclare(queueName);

            lock (_channel)
            {
                _channel.BasicConsume(
                    queue: queueName,
                    autoAck: autoAcknowledge,
                    consumer: consumer);
            }
        }

        public void BasicQos(uint prefetchSize, ushort prefetchCount)
        {
            lock (_channel)
            {
                _channel.BasicQos(
                prefetchSize: prefetchSize,
                prefetchCount: prefetchCount,
                global: false);
            }
        }

        public void BasicAcknowledge(ulong deliveryTag, bool multiple)
        {
            lock (_channel)
            {
                _channel.BasicAck(
                deliveryTag: deliveryTag,
                multiple: multiple);
            }
        }

        public void BasicTopicBind(string queueName, string topic)
        {
            BasicTopicBind(queueName, topic, BusOptions.ExchangeName);
        }

        public void BasicTopicBind(string queueName, string topic, string exchangeName)
        {
            EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));
            if (topic == null) throw new ArgumentNullException(nameof(topic));

            lock (_channel)
            {
                _channel.ExchangeDeclare(exchangeName, BusOptions.ExchangeType);
            }

            QueueDeclare(queueName);

            lock (_channel)
            {
                _channel.QueueBind(queueName, exchangeName, topic);
            }
        }

        public void BasicPublish(EventMessage eventMessage)
        {
            BasicPublish(eventMessage, BusOptions.ExchangeName);
        }

        public void BasicPublish(EventMessage eventMessage, string exchangeName)
        {
            EnsureArg.IsNotNull(eventMessage, nameof(eventMessage));
            EnsureArg.IsNotNullOrWhiteSpace(
                eventMessage.JsonMessage,
                $"{nameof(eventMessage)}.{nameof(eventMessage.JsonMessage)}");

            var properties = _channel.CreateBasicProperties();

            if (eventMessage.CorrelationId != null && eventMessage.CorrelationId != Guid.Empty)
            {
                properties.CorrelationId = eventMessage.CorrelationId.Value.ToString();
            }

            if (!string.IsNullOrWhiteSpace(eventMessage.ReplyQueueName))
            {
                properties.ReplyTo = eventMessage.ReplyQueueName;
            }

            if (!string.IsNullOrWhiteSpace(eventMessage.Type))
            {
                properties.Type = eventMessage.Type;
            }

            if (eventMessage.Timestamp != null)
            {
                properties.Timestamp = new AmqpTimestamp(eventMessage.Timestamp.Value);
            }

            lock (_channel)
            {
                _channel.BasicPublish(exchange: exchangeName,
                                     routingKey: eventMessage.RoutingKey,
                                     basicProperties: properties,
                                     body: Encoding.UTF8.GetBytes(eventMessage.JsonMessage));
            }
        }

        private static void HandleReceivedEvent(BasicDeliverEventArgs args, EventReceivedCallback callback)
        {
            var message = Encoding.UTF8.GetString(args.Body);

            Guid? correlationId = null;

            if (args.BasicProperties.CorrelationId != null &&
               Guid.TryParse(args.BasicProperties.CorrelationId, out Guid parsedId))
            {
                correlationId = parsedId;
            }

            bool? isError = (bool?)args.BasicProperties.Headers?.FirstOrDefault(a => a.Key == "isError").Value;

            var eventMessage = new EventMessage()
            {
                JsonMessage = message,
                RoutingKey = args.RoutingKey,
                CorrelationId = correlationId,
                Timestamp = args.BasicProperties.Timestamp.UnixTime,
                ReplyQueueName = args.BasicProperties.ReplyTo,
                Type = args.BasicProperties.Type,
                IsError = isError != null && isError.Value,
                DeliveryTag = args.DeliveryTag
            };

            callback(eventMessage);
        }

        private void QueueDeclare(string queueName)
        {
            lock (_channel)
            {
                _channel.QueueDeclare(
                    queue: queueName,
                    exclusive: false,
                    autoDelete: false);
            }
        }

        #region IDisposable

        private bool _isDisposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                _connection?.Dispose();
                _channel?.Dispose();
            }

            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable
    }
}
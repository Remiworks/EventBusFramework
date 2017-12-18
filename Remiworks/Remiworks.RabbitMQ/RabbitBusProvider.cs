using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Remiworks.Core;
using Remiworks.Core.Exceptions;
using Remiworks.Core.Models;

namespace Remiworks.RabbitMQ
{
    public class RabbitBusProvider : IBusProvider
    {
        private const string ExchangeType = "topic";

        private IConnection _connection;
        private IModel _channel;

        public BusOptions BusOptions { get; }

        public RabbitBusProvider(BusOptions busOptions)
        {
            EnsureArg.IsNotNull(busOptions, nameof(busOptions));

            BusOptions = busOptions;

            CreateConnection();
        }

        public void CreateConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = BusOptions.Hostname,
                VirtualHost = BusOptions.VirtualHost
            };

            if (BusOptions.Port != null) factory.Port = BusOptions.Port.Value;
            if (BusOptions.UserName != null) factory.UserName = BusOptions.UserName;
            if (BusOptions.Password != null) factory.Password = BusOptions.Password;

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(BusOptions.ExchangeName, ExchangeType);
        }

        public void BasicConsume(string queueName, EventReceivedCallback callback, bool autoAck = true)
        {
            EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));
            EnsureArg.IsNotNull(callback, nameof(callback));

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (sender, args) => HandleReceivedEvent(args, callback);

            QueueDeclare(queueName);
            
            _channel.BasicConsume(
                queue: queueName, 
                autoAck: autoAck, 
                consumer: consumer);
        }

        public void BasicQos(uint prefetchSize, ushort prefetchCount)
        {
            _channel.BasicQos(
                prefetchSize: prefetchSize, 
                prefetchCount: prefetchCount, 
                global: false);
        }

        public void BasicAcknowledge(ulong deliveryTag, bool multiple)
        {
            _channel.BasicAck(
                deliveryTag: deliveryTag, 
                multiple: multiple);
        }

        public void BasicTopicBind(string queueName, params string[] topics)
        {
            EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));
            if (topics == null || !topics.Any()) throw new ArgumentNullException(nameof(topics));

            QueueDeclare(queueName);

            topics
                .Where(t => t != null)
                .ToList()
                .ForEach(topic => _channel.QueueBind(queueName, BusOptions.ExchangeName, topic));
        }

        public void BasicPublish(EventMessage eventMessage)
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

            _channel.BasicPublish(exchange: BusOptions.ExchangeName,
                                 routingKey: eventMessage.RoutingKey,
                                 basicProperties: properties,
                                 body: Encoding.UTF8.GetBytes(eventMessage.JsonMessage));
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
            _channel.QueueDeclare(
                queue: queueName,
                exclusive: false,
                autoDelete: false);
        }

        #region IDisposable
        
        private bool _isDisposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                _connection.Dispose();
                _channel.Dispose();
            }

            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        #endregion
    }
}
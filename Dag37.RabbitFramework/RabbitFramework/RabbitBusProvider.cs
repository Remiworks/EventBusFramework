using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabbitFramework
{
    public class RabbitBusProvider : IBusProvider
    {
        private const string ExchangeType = "topic";

        private IConnection _connection;
        private IModel _channel;
        private readonly IBusHelper _busHelper;

        public BusOptions BusOptions { get; }

        public RabbitBusProvider(BusOptions busOptions, IBusHelper busHelper)
        {
            BusOptions = busOptions;
            _busHelper = busHelper;
        }

        public void BasicPublish(EventMessage message)
        {
            _channel.BasicPublish(exchange: BusOptions.ExchangeName,
                                 routingKey: message.RoutingKey,
                                 basicProperties: null,
                                 body: Encoding.UTF8.GetBytes(message.JsonMessage));
        }

        public void CreateConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = BusOptions.Hostname,
                Port = BusOptions.Port,
                UserName = BusOptions.UserName,
                Password = BusOptions.Password,
                VirtualHost = BusOptions.VirtualHost
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(BusOptions.ExchangeName, ExchangeType);
        }

        public void BasicConsume(string queue, string topic, EventReceivedCallback callback)
        {
            if (string.IsNullOrWhiteSpace(queue))
            {
                throw new ArgumentException("Queue is required");
            }

            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentException(nameof(topic));
            }

            if (callback == null)
            {
                throw new ArgumentException(nameof(callback));
            }

            var queueName = _busHelper.GenerateQueueName(queue, topic);
            var topicName = _busHelper.GenerateTopicName(queue, topic);

            _channel.QueueDeclare(queue: queueName, exclusive: true);

            _channel.QueueBind(queueName, BusOptions.ExchangeName, topicName);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (sender, args) => HandleReceivedEvent(args, callback);

            _channel.BasicConsume(queueName, true, consumer);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private void HandleReceivedEvent(BasicDeliverEventArgs args, EventReceivedCallback callback)
        {
            var message = Encoding.UTF8.GetString(args.Body);

            Guid? correlationId = null;

            if (args.BasicProperties.CorrelationId != null &&
               Guid.TryParse(args.BasicProperties.CorrelationId, out Guid parsedId))
            {
                correlationId = parsedId;
            }

            EventMessage eventMessage = new EventMessage()
            {
                JsonMessage = message,
                RoutingKey = args.RoutingKey,
                CorrelationId = correlationId,
                Timestamp = args.BasicProperties.Timestamp.UnixTime,
                ReplyQueueName = args.BasicProperties.ReplyTo,
                Type = args.BasicProperties.Type
            };

            callback(eventMessage);
        }
    }
}
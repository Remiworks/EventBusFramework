using System;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitFramework.Contracts;
using RabbitFramework.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Reflection;
using System.Collections.Generic;
using RabbitFramework.Publishers;
using System.Threading.Tasks;

namespace RabbitFramework
{
    public class RabbitBusProvider : IBusProvider
    {
        private const string ExchangeType = "topic";

        private IConnection _connection;
        private IModel _channel;

        public BusOptions BusOptions { get; }

        public RabbitBusProvider(BusOptions busOptions)
        {
            BusOptions = busOptions;
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

        public void BasicConsume(string queueName, EventReceivedCallback callback)
        {
            if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentNullException(nameof(queueName));
            else if (callback == null) throw new ArgumentNullException(nameof(callback));

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (sender, args) => HandleReceivedEvent(args, callback);

            QueueDeclare(queueName);
            _channel.BasicConsume(queueName, true, consumer);
        }

        public void CreateTopicsForQueue(string queueName, params string[] topics)
        {
            if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentNullException(nameof(queueName));
            else if (topics == null || !topics.Any() || topics.Any(t => t == null)) throw new ArgumentNullException(nameof(topics));

            QueueDeclare(queueName);

            topics.ToList().ForEach(topic =>
                _channel.QueueBind(queueName, BusOptions.ExchangeName, topic));
        }

        public void BasicPublish(EventMessage eventMessage)
        {
            if (eventMessage == null) throw new ArgumentNullException(nameof(eventMessage));

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

        public void SetupRpcListeners(string queueName, string[] keys, CommandReceivedCallback function)
        {
            if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentNullException($"The {nameof(queueName)} should not be null");
            else if (function == null) throw new ArgumentNullException($"The {nameof(function)} should not be null");

            _channel.QueueDeclare(
                queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.BasicQos(0, 1, false);

            keys.ToList().ForEach(key =>
                _channel.QueueBind(queueName, BusOptions.ExchangeName, key));

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (sender, args) => Task.Run(() => HandleReceivedCommand(function, args));

            _channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer);
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

            bool? isError = (bool?)args.BasicProperties.Headers?.FirstOrDefault(a => a.Key == "isError").Value;

            EventMessage eventMessage = new EventMessage()
            {
                JsonMessage = message,
                RoutingKey = args.RoutingKey,
                CorrelationId = correlationId,
                Timestamp = args.BasicProperties.Timestamp.UnixTime,
                ReplyQueueName = args.BasicProperties.ReplyTo,
                Type = args.BasicProperties.Type,
                IsError = isError != null ? (bool)isError : false

            };

            callback(eventMessage);
        }

        private async Task HandleReceivedCommand(CommandReceivedCallback function, BasicDeliverEventArgs args)
        {

            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = args.BasicProperties.CorrelationId;

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

            string response = await InvokeCommandReceivedCallback(function, replyProps, eventMessage);

            var responseBytes = Encoding.UTF8.GetBytes(response);

            _channel.BasicPublish(
                exchange: "",
                routingKey: args.BasicProperties.ReplyTo,
                basicProperties: replyProps,
                body: responseBytes);

            _channel.BasicAck(
                deliveryTag: args.DeliveryTag,
                multiple: false);

        }

        private async Task<string> InvokeCommandReceivedCallback(CommandReceivedCallback function, IBasicProperties replyProps, EventMessage eventMessage)
        {
            var response = "";
            replyProps.Headers = new Dictionary<string, object>();

            try
            {
                response = await function(eventMessage);
                replyProps.Headers.Add("isError", false);
            }
            catch (TargetInvocationException ex)
            {
                var exception = new CommandPublisherException(ex.InnerException.Message, ex.InnerException);
                response = JsonConvert.SerializeObject(exception);
                replyProps.Headers.Add("isError", true);
            }
            catch(Exception ex)
            {
                var exception = new CommandPublisherException(ex.Message, ex);
                response = JsonConvert.SerializeObject(exception);
                replyProps.Headers.Add("isError", true);
            }
            return response;
        }

        private void QueueDeclare(string queueName)
        {
            _channel.QueueDeclare(
                queue: queueName,
                exclusive: false,
                autoDelete: false);
        }

        private bool isDisposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    _connection.Dispose();
                    _channel.Dispose();
                }

                isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
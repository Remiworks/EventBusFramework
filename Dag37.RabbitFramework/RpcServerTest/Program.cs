using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace RpcServerTest
{
    public static class Program
    {
        private static IConnection _connection;
        private static IModel _channel;

        public static void Main()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            SetupRpcListener<SomeCommand>("rpc_queue", Fib);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();

            _connection.Dispose();
            _channel.Dispose();
        }

        private static void SetupRpcListener<TParam>(string queue, Func<TParam, object> function)
        {
            _channel.QueueDeclare(
                queue: queue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (sender, args) => Consumer_Received(function, args);

            _channel.BasicConsume(
                queue: queue,
                autoAck: false,
                consumer: consumer);
        }

        private static void Consumer_Received<TParam>(Func<TParam, object> function, BasicDeliverEventArgs args)
        {
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = args.BasicProperties.CorrelationId;

            var bodyJson = Encoding.UTF8.GetString(args.Body);
            TParam bodyObject = JsonConvert.DeserializeObject<TParam>(bodyJson);

            object functionResult = function(bodyObject);
            string response = JsonConvert.SerializeObject(functionResult);

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

        private static object Fib(SomeCommand n)
        {
            return CalculateFib(n.Value);
        }

        private static int CalculateFib(int n)
        {
            if (n == 0 || n == 1)
            {
                return n;
            }

            return CalculateFib(n - 1) + CalculateFib(n - 2);
        }
    }

    public class SomeCommand
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}
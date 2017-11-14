using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

namespace RpcTest
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            var rpcClient = new RpcClient();

            var command = new SomeCommand { Name = "Something", Value = 10 };
            Console.WriteLine($" [x] Requesting fib({command.Value})");

            int response = rpcClient.Call<int>(command);
            Console.WriteLine(" [.] Got '{0}'", response);

            rpcClient.Dispose();
        }
    }

    public class RpcClient
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;

        public RpcClient()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            var replyQueueName = _channel.QueueDeclare().QueueName;

            // End of initialization

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var response = Encoding.UTF8.GetString(body);

                var correlationId = ea.BasicProperties.CorrelationId;
                
                if(stuff.ContainsKey(correlationId))
                {
                    stuff[correlationId].Set();
                }
            };

            _channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true);
        }

        ConcurrentDictionary<string, ManualResetEvent> stuff = new ConcurrentDictionary<string, ManualResetEvent>();

        public T Call<T>(object message)
        {
            var correlationId = Guid.NewGuid().ToString();
            var resetEvent = new ManualResetEvent(false);

            stuff[correlationId] = resetEvent;

            string messageJson = JsonConvert.SerializeObject(message);
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);





            var props = _channel.CreateBasicProperties();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            _channel.BasicPublish(
                exchange: "",
                routingKey: "rpc_queue",
                basicProperties: props,
                body: messageBytes);

            string responseJson = respQueue.Take();

            return JsonConvert.DeserializeObject<T>(responseJson);
        }

        public void Dispose()
        {
            _connection.Dispose();
            _channel.Dispose();
        }
    }

    public class SomeCommand
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}
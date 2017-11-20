using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RpcTest
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            var rpcClient = new RpcClient();

            var command = new SomeCommand { Name = "Something", Value = 10 };
            Console.WriteLine($" [x] Requesting fib({command.Value})");

            Task<string> responseTask = rpcClient.Call<string>(command);

            for (int i = 0; i < 1000; i++)
            {
                Console.WriteLine($"Something else {i}");
            }

            string response = responseTask.Result;
            Console.WriteLine(" [.] Got '{0}'", response);

            Console.ReadLine();

            rpcClient.Dispose();
        }
    }

    public class RpcClient : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _replyQueueName;
        private readonly EventingBasicConsumer _consumer;

        private readonly ConcurrentDictionary<string, Action<string>> _stuff;

        public RpcClient()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _replyQueueName = _channel.QueueDeclare().QueueName;

            _stuff = new ConcurrentDictionary<string, Action<string>>();

            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += HandleResponse;
        }

        public async Task<T> Call<T>(object message)
        {
            var correlationId = Guid.NewGuid().ToString();

            var waitHandle = new ManualResetEvent(false);
            string responseJson = null;

            _stuff[correlationId] = (resp) =>
            {
                responseJson = resp;
                waitHandle.Set();
            };

            SendCommand(correlationId, message);
            Task<bool> waitForHandle = Task.Run(() => waitHandle.WaitOne(5000));

            bool gotResponse = await waitForHandle;

            return gotResponse
                ? JsonConvert.DeserializeObject<T>(responseJson)
                : throw new TimeoutException();
        }

        private void SendCommand(string correlationId, object message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            _channel.BasicPublish(
                exchange: "",
                routingKey: "rpc_queue",
                basicProperties: CreateProperties(correlationId),
                body: messageBytes);

            _channel.BasicConsume(
                consumer: _consumer,
                queue: _replyQueueName,
                autoAck: true);
        }

        private IBasicProperties CreateProperties(string correlationId)
        {
            var properties = _channel.CreateBasicProperties();
            properties.CorrelationId = correlationId;
            properties.ReplyTo = _replyQueueName;

            return properties;
        }

        private void HandleResponse(object model, BasicDeliverEventArgs eventArgs)
        {
            var body = eventArgs.Body;
            string responsejson = Encoding.UTF8.GetString(body);

            string correlationId = eventArgs.BasicProperties.CorrelationId;

            if (_stuff.ContainsKey(correlationId))
            {
                _stuff[correlationId](responsejson);
            }
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
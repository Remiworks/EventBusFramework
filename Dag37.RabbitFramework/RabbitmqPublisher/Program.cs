using System;

namespace RabbitmqPublisher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
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
    }
}

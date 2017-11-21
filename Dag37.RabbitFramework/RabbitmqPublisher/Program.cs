using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitmqReceiver;
using System;
using System.Text;

namespace RabbitmqPublisher
{
    public class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "testExchange",
                                        type: "topic");

                UserModel userModel = new UserModel() { Name = "test" };
                var message = JsonConvert.SerializeObject(userModel);
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "testExchange",
                                     routingKey: "user.event.created",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent '{0}':'{1}'");
            }
        }
    }
}

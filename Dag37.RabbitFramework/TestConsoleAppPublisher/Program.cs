using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading;

namespace TestConsoleAppPublisher
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

                while (true)
                {
                    var routingKey = "user.event.deleted";
                    var json = @"{'Name': 'Bad Boys'}";
                    var body = Encoding.UTF8.GetBytes(json);
                    channel.BasicPublish(exchange: "testExchange",
                                         routingKey: routingKey,
                                         basicProperties: null,
                                         body: body);
                
                Console.WriteLine(" [x] Sent '{0}':'{1}'", routingKey, json);
                    Thread.Sleep(1000);
                }
            }
        }
    }
}

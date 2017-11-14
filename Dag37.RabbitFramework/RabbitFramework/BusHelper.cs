namespace RabbitFramework
{
    public class BusHelper : IBusHelper
    {
        public string GenerateQueueName(string queue, string topic)
        {
            var formattedTopic = topic.Replace('.', '-');

            return $"{queue}-{formattedTopic}";
        }

        public string GenerateTopicName(string queue, string topic)
        {
            return $"{queue}.{topic}";
        }
    }
}
namespace RabbitFramework
{
    public interface IBusHelper
    {
        string GenerateQueueName(string queue, string topic);
        string GenerateTopicName(string queue, string topic);
    }
}
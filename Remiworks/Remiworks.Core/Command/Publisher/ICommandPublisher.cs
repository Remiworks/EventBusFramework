using System.Threading.Tasks;

namespace Remiworks.Core.Command.Publisher
{
    public interface ICommandPublisher
    {
        Task<T> SendCommandAsync<T>(object message, string queueName, string key, int timeout = 5000);
        Task<T> SendCommandAsync<T>(object message, string queueName, string key, string exchangeName, int timeout = 5000);

        Task SendCommandAsync(object message, string queueName, string key, int timeout = 5000);
        Task SendCommandAsync(object message, string queueName, string key, string exchangeName, int timeout = 5000);
    }
}
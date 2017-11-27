using RabbitFramework.Models;
using System.Threading.Tasks;

namespace RabbitFramework.Contracts
{
    public interface ICommandPublisher
    {
        Task<T> SendCommandAsync<T>(object message, string queueName, string key, int timeout = 5000);
    }
}
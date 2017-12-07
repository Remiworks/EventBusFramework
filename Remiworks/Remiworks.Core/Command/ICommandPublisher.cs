using System.Threading.Tasks;

namespace Remiworks.Core.Command
{
    public interface ICommandPublisher
    {
        Task<T> SendCommandAsync<T>(object message, string queueName, string key, int timeout = 5000);
    }
}
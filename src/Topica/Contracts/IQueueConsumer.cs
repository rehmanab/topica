using System.Threading;
using System.Threading.Tasks;
using Topica.Messages;

namespace Topica.Contracts
{
    public interface IQueueConsumer
    {
        Task StartAsync<T>(string consumerName, string source, CancellationToken cancellationToken = default) where T : Message;
    }
}
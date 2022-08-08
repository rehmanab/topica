using System.Threading;
using System.Threading.Tasks;
using Topica.Messages;
using Topica.Settings;

namespace Topica.Contracts
{
    public interface IConsumer
    {
        Task StartAsync<T>(string consumerName, ConsumerItemSettings consumerItemSettings, CancellationToken cancellationToken = default) where T : Message;
    }
}
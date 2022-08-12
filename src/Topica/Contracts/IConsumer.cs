using System.Threading;
using System.Threading.Tasks;
using Topica.Messages;
using Topica.Settings;

namespace Topica.Contracts
{
    public interface IConsumer
    {
        Task ConsumeAsync<T>(string consumerName, ConsumerItemSettings consumerItemSettings, int numberOfInstances, CancellationToken cancellationToken = default) where T : Message;
        Task ConsumeAsync<T>(string consumerName, ConsumerItemSettings consumerItemSettings, CancellationToken cancellationToken = default) where T : Message;
    }
}
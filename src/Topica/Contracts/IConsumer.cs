using System.Threading;
using System.Threading.Tasks;
using Topica.Settings;

namespace Topica.Contracts
{
    public interface IConsumer
    {
        Task ConsumeAsync(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken);
    }
}
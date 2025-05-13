using System.Threading;
using System.Threading.Tasks;
using Topica.Settings;

namespace Topica.Contracts
{
    public interface IConsumer
    {
        Task ConsumeAsync<T>(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken) where T : IHandler;
        
        //TODO - remove this when we fully using the generic method
        Task ConsumeAsync(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken);
    }
}
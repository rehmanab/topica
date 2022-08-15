using System.Threading;
using System.Threading.Tasks;
using Topica.Settings;

namespace Topica.Contracts
{
    public interface IProducerBuilder
    {
        Task<T> BuildProducerAsync<T>(string producerName, ProducerSettings producerSettings, CancellationToken cancellationToken);
    }
}
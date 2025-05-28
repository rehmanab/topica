using System.Threading;
using System.Threading.Tasks;

namespace Topica.Contracts
{
    public interface IConsumer
    {
        Task ConsumeAsync<T>(CancellationToken cancellationToken) where T : IHandler;
    }
}
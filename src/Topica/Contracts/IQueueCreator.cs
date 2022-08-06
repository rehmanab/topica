using System.Threading.Tasks;
using Topica.Config;

namespace Topica.Contracts
{
    public interface IQueueCreator
    {
        Task<string> CreateQueue(string queueName, SqsConfiguration configuration);
    }
}
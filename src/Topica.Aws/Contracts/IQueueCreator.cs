using System.Threading.Tasks;
using Topica.Aws.Queues;

namespace Topica.Aws.Contracts
{
    public interface IQueueCreator
    {
        Task<string> CreateQueue(string queueName, SqsConfiguration? configuration);
    }
}
using System.Threading.Tasks;
using Topica.Aws.Queues;

namespace Topica.Aws.Contracts
{
    public interface IAwsQueueCreator
    {
        Task<string> CreateQueue(string queueName, AwsSqsConfiguration configuration);
    }
}
using System.Threading.Tasks;
using Aws.Messaging.Config;

namespace Aws.Messaging.Contracts
{
    public interface IQueueCreator
    {
        Task<string> CreateQueue(string queueName, SqsConfiguration configuration);
    }
}
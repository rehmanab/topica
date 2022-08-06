using System.Threading.Tasks;

namespace Topica.Aws.Queues
{
    public interface IQueueCreator
    {
        Task<string> CreateQueue(string queueName, QueueConfiguration? configuration);
    }
}
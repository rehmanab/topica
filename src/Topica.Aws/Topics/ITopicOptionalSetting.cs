using System.Collections.Generic;
using System.Threading.Tasks;
using Topica.Aws.Queues;

namespace Topica.Aws.Topics
{
    public interface ITopicOptionalSetting
    {
        ITopicOptionalSetting WithSubscribedQueue(string queueName);
        ITopicOptionalSetting WithSubscribedQueue(IEnumerable<string> queueNames);
        ITopicOptionalSetting WithQueueConfiguration(QueueConfiguration? sqsConfiguration);
        Task<string> BuildAsync();
    }
}
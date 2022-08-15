using System.Collections.Generic;
using System.Threading.Tasks;
using Topica.Aws.Queues;

namespace Topica.Aws.Contracts
{
    public interface IAwsTopicOptionalSetting
    {
        IAwsTopicOptionalSetting WithSubscribedQueue(string queueName);
        IAwsTopicOptionalSetting WithSubscribedQueue(IEnumerable<string> queueNames);
        IAwsTopicOptionalSetting WithQueueConfiguration(QueueConfiguration? sqsConfiguration);
        Task<string> BuildAsync();
    }
}
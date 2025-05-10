using System.Collections.Generic;
using System.Threading.Tasks;
using Topica.Aws.Queues;
using Topica.Messages;

namespace Topica.Aws.Contracts
{
    public interface IAwsTopicService
    {
        Task<string?> GetTopicArnAsync(string topicName, bool isFifo);
        Task<bool> TopicExistsAsync(string topicName);
        Task AuthorizeS3ToPublishByTopicNameAsync(string topicName, string bucketName);
        Task AuthorizeS3ToPublishByTopicArnAsync(string topicArn, string bucketName);
        Task<string?> CreateTopicArnAsync(string topicName, bool isFifoQueue);
        Task SendToTopicAsync(string topicArn, Message message);
        Task SendToTopicByTopicNameAsync(string topicName, Message message);
        Task<bool> SubscriptionExistsAsync(string topicArn, string endpointArn);
        Task<IEnumerable<string>> ListTopicSubscriptionsAsync(string topicArn);
        Task<string?> CreateTopicWithOptionalQueuesSubscribedAsync(string topicName, string[] queueNames);
        Task<string?> CreateTopicWithOptionalQueuesSubscribedAsync(string topicName, string[] queueNames, QueueConfiguration? sqsConfiguration);
        Task<bool> DeleteTopicArnAsync(string topicName);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Topica.Aws.Messages;
using Topica.Aws.Queues;

namespace Topica.Aws.Topics
{
    public interface ITopicProvider
    {
        Task<string?> GetTopicArnAsync(string topicName);
        Task<bool> TopicExistsAsync(string topicName);
        Task AuthorizeS3ToPublishByTopicNameAsync(string topicName, string bucketName);
        Task AuthorizeS3ToPublishByTopicArnAsync(string topicArn, string bucketName);
        Task<string?> CreateTopicArnAsync(string topicName, bool? isFifoQueue);
        Task SendToTopicAsync(string topicArn, BaseSqsMessage message);
        Task SendToTopicByTopicNameAsync(string topicName, BaseSqsMessage message);
        Task<bool> SubscriptionExistsAsync(string topicArn, string endpointArn);
        Task<IEnumerable<string>> ListTopicSubscriptionsAsync(string topicArn);
        Task<string?> CreateTopicWithOptionalQueuesSubscribedAsync(string topicName, string[] queueNames);
        Task<string?> CreateTopicWithOptionalQueuesSubscribedAsync(string topicName, string[] queueNames, QueueConfiguration? sqsConfiguration);
        Task<bool> DeleteTopicArnAsync(string topicName);
    }
}
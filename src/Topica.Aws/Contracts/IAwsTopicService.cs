using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService.Model;
using Topica.Aws.Queues;
using Topica.Messages;

namespace Topica.Aws.Contracts
{
    public interface IAwsTopicService
    {
        IAsyncEnumerable<IEnumerable<Topic>> GetAllTopics(string? topicNamePrefix = null, bool? isFifo = false);
        Task<string?> GetTopicArnAsync(string topicName, bool isFifo);
        Task<bool> TopicExistsAsync(string topicName);
        Task AuthorizeS3ToPublishByTopicNameAsync(string topicName, string bucketName);
        Task AuthorizeS3ToPublishByTopicArnAsync(string topicArn, string bucketName);
        Task<string?> CreateTopicArnAsync(string topicName, bool isFifoQueue);
        Task SendToTopicAsync(string topicArn, BaseMessage message);
        Task SendToTopicByTopicNameAsync(string topicName, BaseMessage message);
        Task<bool> SubscriptionExistsAsync(string topicArn, string endpointArn);
        Task<IEnumerable<string>> ListTopicSubscriptionsAsync(string topicArn);
        Task<string?> CreateTopicWithOptionalQueuesSubscribedAsync(string topicName, string[] queueNames);
        Task<string?> CreateTopicWithOptionalQueuesSubscribedAsync(string topicName, string[] queueNames, AwsSqsConfiguration? sqsConfiguration);
        Task<bool> DeleteTopicArnAsync(string topicName);
    }
}
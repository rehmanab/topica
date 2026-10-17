using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService.Model;
using Topica.Aws.Queues;

namespace Topica.Aws.Contracts
{
    public interface IAwsTopicService
    {
        IAsyncEnumerable<IEnumerable<Topic>> GetAllTopics(string? topicName = null, bool? isFifo = false);
        Task<string?> GetTopicArnAsync(string topicName, bool isFifo);
        IAsyncEnumerable<IEnumerable<Subscription>> GetAllSubscriptions(string topicName, bool isFifo = false);
        IAsyncEnumerable<IEnumerable<Subscription>> GetSubscriptions(string topicArn);
        Task<bool> TopicExistsAsync(string topicName);
        Task AuthorizeS3ToPublishByTopicNameAsync(string topicName, string bucketName);
        Task AuthorizeS3ToPublishByTopicArnAsync(string topicArn, string bucketName);
        Task<string?> CreateTopicArnAsync(string topicName, bool isFifo);
        Task<bool> SubscriptionExistsAsync(string topicArn, string endpointArn);
        Task<string> CreateTopicWithOptionalQueuesSubscribedAsync(string topicName, string[] queueNames, AwsSqsConfiguration sqsConfiguration, CancellationToken cancellationToken);
        string BuildQueueAllowPolicyForTopicToSendMessage(string queueArn, string topicArn);
        string CreateResourceArnWildcard(string resourceArn);
    }
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService.Model;
using Topica.Aws.Queues;

namespace Topica.Aws.Contracts
{
    public interface IAwsTopicService
    {
        IAsyncEnumerable<IEnumerable<Topic>> GetAllTopics(string? topicNamePrefix = null, bool? isFifo = false);
        Task<string?> GetTopicArnAsync(string topicName, bool isFifo);
        Task<bool> TopicExistsAsync(string topicName);
        Task AuthorizeS3ToPublishByTopicNameAsync(string topicName, string bucketName);
        Task AuthorizeS3ToPublishByTopicArnAsync(string topicArn, string bucketName);
        Task<string?> CreateTopicArnAsync(string topicName, bool isFifo);
        Task<bool> SubscriptionExistsAsync(string topicArn, string endpointArn);
        Task<string> CreateTopicWithOptionalQueuesSubscribedAsync(string topicName, string[] queueNames, AwsSqsConfiguration sqsConfiguration, CancellationToken cancellationToken = default);
    }
}
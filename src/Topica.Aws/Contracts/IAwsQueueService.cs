using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Topica.Aws.Queues;
using Topica.Messages;

namespace Topica.Aws.Contracts
{
    public interface IAwsQueueService
    {
        Task<bool> QueueExistsByNameAsync(string queueName);
        Task<bool> QueueExistsByUrlAsync(string queueUrl);
        Task<string?> GetQueueUrlAsync(string queueName);
        Task<IEnumerable<string>> GetQueueNamesByPrefix(string queueNamePrefix = "");
        Task<IEnumerable<string>> GetQueueUrlsByPrefix(string queueNamePrefix = "");
        Task<IDictionary<string, string>> GetAttributesByQueueName(string queueName, IEnumerable<string>? attributeNames = null);
        Task<IDictionary<string, string>> GetAttributesByQueueUrl(string queueUrl, IEnumerable<string>? attributeNames = null);
        Task<string> CreateQueueAsync(string queueName, AwsQueueCreationType awsQueueCreationType);
        IAsyncEnumerable<string>  CreateQueuesAsync(IEnumerable<string> queueNames, AwsSqsConfiguration awsSqsConfiguration);
        Task<string> CreateQueueAsync(string queueName, AwsSqsConfiguration awsSqsConfiguration);
        Task<bool> UpdateQueueAttributesAsync(string queueUrl, AwsSqsConfiguration configuration);
        Task<bool> SendSingleAsync<T>(string queueUrl, T message);
        Task<bool> SendMultipleAsync<T>(string queueUrl, IEnumerable<T> messages);
        IAsyncEnumerable<T> StartReceive<T>(string queueUrl, CancellationToken cancellationToken = default) where T : BaseMessage;
        Task<bool> DeleteMessageAsync(string queueUrl, string receiptHandle);
    }
}
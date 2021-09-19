using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aws.Messaging.Config;
using Aws.Messaging.Messages;
using Aws.Messaging.Queue.SQS;

namespace Aws.Messaging.Contracts
{
    public interface IQueueProvider
    {
        Task<bool> QueueExistsByNameAsync(string queueName);
        Task<bool> QueueExistsByUrlAsync(string queueUrl);
        Task<string> GetQueueUrlAsync(string queueName);
        Task<IEnumerable<string>> GetQueueNamesByPrefix(string queueNamePrefix = "");
        Task<IEnumerable<string>> GetQueueUrlsByPrefix(string queueNamePrefix = "");
        Task<IDictionary<string, string>> GetAttributesByQueueName(string queueName, IEnumerable<string> attributeNames = null);
        Task<IDictionary<string, string>> GetAttributesByQueueUrl(string queueUrl, IEnumerable<string> attributeNames = null);
        Task<string> CreateQueueAsync(string queueName, QueueCreationType queueCreationType);
        Task<string> CreateQueueAsync(string queueName, SqsConfiguration sqsConfiguration);
        Task<bool> UpdateQueueAttributesAsync(string queueUrl, SqsConfiguration configuration);
        Task SendSingleAsync(string queueUrl, BaseSqsMessage message);
        Task SendMultipleAsync(string queueUrl, IEnumerable<BaseSqsMessage> messages);
        IAsyncEnumerable<T> StartReceive<T>(string queueUrl, CancellationToken cancellationToken = default(CancellationToken)) where T : BaseSqsMessage;
        Task<bool> DeleteMessageAsync(string queueUrl, string receiptHandle);
    }
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Topica.Aws.Queues;

namespace Topica.Aws.Contracts
{
    public interface IAwsQueueService
    {
        Task<string?> GetQueueUrlAsync(string queueName, bool isFifo, CancellationToken cancellationToken = default);
        Task<IDictionary<string, string>> GetAttributesByQueueUrl(string queueUrl, IEnumerable<string>? attributeNames = null);
        Task<string> CreateQueueAsync(string queueName, AwsSqsConfiguration awsSqsConfiguration, CancellationToken cancellationToken = default);
        Task<bool> UpdateQueueAttributesAsync(string queueUrl, AwsSqsConfiguration configuration);
        Task<bool> DeleteMessageAsync(string queueUrl, string receiptHandle);
    }
}
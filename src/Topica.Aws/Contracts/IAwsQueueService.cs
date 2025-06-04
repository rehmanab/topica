using System.Collections.Generic;
using System.Threading.Tasks;
using Topica.Aws.Queues;

namespace Topica.Aws.Contracts
{
    public interface IAwsQueueService
    {
        Task<string?> GetQueueUrlAsync(string queueName);
        Task<IDictionary<string, string>> GetAttributesByQueueUrl(string queueUrl, IEnumerable<string>? attributeNames = null);
        Task<string> CreateQueueAsync(string queueName, AwsSqsConfiguration awsSqsConfiguration);
        Task<bool> UpdateQueueAttributesAsync(string queueUrl, AwsSqsConfiguration configuration);
        Task<bool> DeleteMessageAsync(string queueUrl, string receiptHandle);
    }
}
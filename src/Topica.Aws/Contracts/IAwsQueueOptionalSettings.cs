using System.Collections.Generic;
using System.Threading.Tasks;
using Topica.Aws.Queues;

namespace Topica.Aws.Contracts
{
    public interface IAwsQueueOptionalSettings
    {
        IAwsQueueOptionalSettings WithSqsConfiguration(AwsSqsConfiguration? sqsConfiguration);
        Task<IEnumerable<string>> BuildAsync();
    }
}
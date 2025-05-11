using System.Collections.Generic;
using System.Threading.Tasks;
using Topica.Aws.Queues;

namespace Topica.Aws.Contracts
{
    public interface IQueueOptionalSettings
    {
        IQueueOptionalSettings WithSqsConfiguration(SqsConfiguration? sqsConfiguration);
        Task<IEnumerable<string>> BuildAsync();
    }
}
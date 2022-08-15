using System.Collections.Generic;
using System.Threading.Tasks;
using Topica.Aws.Queues;

namespace Topica.Aws.Contracts
{
    public interface IQueueOptionalSettings
    {
        IQueueOptionalSettings WithQueueConfiguration(QueueConfiguration? sqsConfiguration);
        Task<IEnumerable<string>> BuildAsync();
    }
}
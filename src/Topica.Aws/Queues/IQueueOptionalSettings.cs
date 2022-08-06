using System.Collections.Generic;
using System.Threading.Tasks;

namespace Topica.Aws.Queues
{
    public interface IQueueOptionalSettings
    {
        IQueueOptionalSettings WithQueueConfiguration(QueueConfiguration? sqsConfiguration);
        Task<IEnumerable<string>> BuildAsync();
    }
}
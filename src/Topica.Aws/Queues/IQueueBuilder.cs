using System.Collections.Generic;

namespace Topica.Aws.Queues
{
    public interface IQueueBuilder
    {
        IQueueOptionalSettings WithQueueName(string queueName);
        IQueueOptionalSettings WithQueueNames(IEnumerable<string> queueNames);
    }
}
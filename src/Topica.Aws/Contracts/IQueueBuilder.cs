using System.Collections.Generic;

namespace Topica.Aws.Contracts
{
    public interface IQueueBuilder
    {
        IQueueOptionalSettings WithQueueName(string queueName);
        IQueueOptionalSettings WithQueueNames(IEnumerable<string> queueNames);
    }
}
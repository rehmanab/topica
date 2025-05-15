using System.Collections.Generic;

namespace Topica.Aws.Contracts
{
    public interface IAwsQueueBuilder
    {
        IAwsQueueOptionalSettings WithQueueName(string queueName);
        IAwsQueueOptionalSettings WithQueueNames(IEnumerable<string> queueNames);
    }
}
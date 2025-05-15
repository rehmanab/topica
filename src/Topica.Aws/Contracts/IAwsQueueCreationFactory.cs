using Topica.Aws.Queues;

namespace Topica.Aws.Contracts
{
    public interface IAwsQueueCreationFactory
    {
        IAwsQueueCreator Create(AwsQueueCreationType awsQueueCreationType);
    }
}
using Topica.Config;
using Topica.Queue;

namespace Topica.Contracts
{
    public interface ISqsConfigurationBuilder
    {
        SqsConfiguration BuildCreateDefaultQueue();
        SqsConfiguration BuildCreateDefaultQueue(AwsQueueAttributes awsQueueAttributes);
        SqsConfiguration BuildCreateWithErrorQueue();
        SqsConfiguration BuildCreateWithErrorQueue(int maxReceiveCount);
        SqsConfiguration BuildCreateWithErrorQueue(int maxReceiveCount, AwsQueueAttributes awsQueueAttributes);
        SqsConfiguration BuildWithCreationTypeQueue(QueueCreationType queueCreationType);
        SqsConfiguration BuildUpdatePolicyQueue(string policy);
    }
}
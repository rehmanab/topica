using Topica.Aws.Queues;

namespace Topica.Aws.Configuration
{
    public interface ISqsConfigurationBuilder
    {
        QueueConfiguration? BuildCreateDefaultQueue();
        QueueConfiguration? BuildCreateDefaultQueue(AwsQueueAttributes awsQueueAttributes);
        QueueConfiguration? BuildCreateWithErrorQueue();
        QueueConfiguration? BuildCreateWithErrorQueue(int maxReceiveCount);
        QueueConfiguration? BuildCreateWithErrorQueue(int maxReceiveCount, AwsQueueAttributes awsQueueAttributes);
        QueueConfiguration? BuildWithCreationTypeQueue(QueueCreationType queueCreationType);
        QueueConfiguration BuildUpdatePolicyQueue(string policy);
    }
}
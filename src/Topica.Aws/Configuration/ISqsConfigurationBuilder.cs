using Topica.Aws.Queues;

namespace Topica.Aws.Configuration
{
    public interface ISqsConfigurationBuilder
    {
        QueueConfiguration BuildQueue();
        QueueConfiguration BuildQueue(AwsQueueAttributes awsQueueAttributes);
        QueueConfiguration BuildDefaultQueueWithErrorQueue();
        QueueConfiguration BuildDefaultQueueWithErrorQueue(int maxReceiveCount);
        QueueConfiguration BuildQueueWithErrorQueue(int maxReceiveCount, AwsQueueAttributes awsQueueAttributes);
        QueueConfiguration BuildWithCreationTypeQueue(QueueCreationType queueCreationType);
        QueueConfiguration BuildUpdatePolicyQueue(string policy);
    }
}
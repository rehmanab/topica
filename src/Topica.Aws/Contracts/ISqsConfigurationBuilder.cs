using Topica.Aws.Queues;

namespace Topica.Aws.Contracts
{
    public interface ISqsConfigurationBuilder
    {
        SqsConfiguration BuildQueue();
        SqsConfiguration BuildQueue(AwsQueueAttributes awsQueueAttributes);
        SqsConfiguration BuildDefaultQueueWithErrorQueue();
        SqsConfiguration BuildDefaultQueueWithErrorQueue(int maxReceiveCount);
        SqsConfiguration BuildQueueWithErrorQueue(int maxReceiveCount, AwsQueueAttributes awsQueueAttributes);
        SqsConfiguration BuildWithCreationTypeQueue(QueueCreationType queueCreationType);
        SqsConfiguration BuildUpdatePolicyQueue(string policy);
    }
}
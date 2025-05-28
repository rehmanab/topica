using Topica.Aws.Queues;

namespace Topica.Aws.Contracts
{
    public interface IAwsSqsConfigurationBuilder
    {
        AwsSqsConfiguration BuildQueue();
        AwsSqsConfiguration BuildQueue(AwsQueueAttributes awsQueueAttributes);
        AwsSqsConfiguration BuildDefaultQueueWithErrorQueue();
        AwsSqsConfiguration BuildDefaultQueueWithErrorQueue(int maxReceiveCount);
        AwsSqsConfiguration BuildQueueWithErrorQueue(int errorQueueMaxReceiveCount, AwsQueueAttributes awsQueueAttributes);
        AwsSqsConfiguration BuildWithCreationTypeQueue(AwsQueueCreationType awsQueueCreationType);
        AwsSqsConfiguration BuildUpdatePolicyQueue(string policy);
    }
}
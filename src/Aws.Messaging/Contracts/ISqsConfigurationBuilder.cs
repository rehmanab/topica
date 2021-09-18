using Aws.Messaging.Config;
using Aws.Messaging.Queue.SQS;

namespace Aws.Messaging.Contracts
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
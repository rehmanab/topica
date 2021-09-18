using Aws.Messaging.Queue.SQS;

namespace Aws.Messaging.Contracts
{
    public interface IQueueCreationFactory
    {
        IQueueCreator Create(QueueCreationType queueCreationType);
    }
}
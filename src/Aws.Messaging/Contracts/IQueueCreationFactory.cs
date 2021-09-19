using Aws.Messaging.Queue;

namespace Aws.Messaging.Contracts
{
    public interface IQueueCreationFactory
    {
        IQueueCreator Create(QueueCreationType queueCreationType);
    }
}
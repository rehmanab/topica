using Topica.Aws.Queues;

namespace Topica.Aws.Contracts
{
    public interface IQueueCreationFactory
    {
        IQueueCreator Create(QueueCreationType queueCreationType);
    }
}
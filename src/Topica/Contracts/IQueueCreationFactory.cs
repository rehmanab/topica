using Topica.Queue;

namespace Topica.Contracts
{
    public interface IQueueCreationFactory
    {
        IQueueCreator Create(QueueCreationType queueCreationType);
    }
}
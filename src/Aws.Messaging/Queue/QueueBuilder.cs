using Aws.Messaging.Contracts;

namespace Aws.Messaging.Queue
{
    public interface IQueueBuilder
    {
        IQueueOptionalSettings WithQueueName(string queueName);
    }
    
    public class QueueBuilder : IQueueBuilder
    {
        private readonly IQueueProvider _queueProvider;

        public QueueBuilder(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }
        
        public IQueueOptionalSettings WithQueueName(string queueName)
        {
            return new QueueOptionalSettings(queueName, _queueProvider); 
        }
    }
}
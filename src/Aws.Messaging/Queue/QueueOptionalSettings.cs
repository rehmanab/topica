using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aws.Messaging.Config;
using Aws.Messaging.Contracts;

namespace Aws.Messaging.Queue
{
    public interface IQueueOptionalSettings
    {
        IQueueOptionalSettings WithQueueConfiguration(SqsConfiguration sqsConfiguration);
        Task<IEnumerable<string>> BuildAsync();
    }
    
    public class QueueOptionalSettings : IQueueOptionalSettings
    {
        private readonly IEnumerable<string> _queueNames;
        private readonly IQueueProvider _queueProvider;
        private SqsConfiguration _sqsConfiguration;

        public QueueOptionalSettings(IQueueProvider queueProvider, IEnumerable<string> queueNames)
        {
            _queueProvider = queueProvider;
            _queueNames = queueNames;
        }
        
        public QueueOptionalSettings(IQueueProvider queueProvider, string queueName)
        {
            _queueProvider = queueProvider;
            _queueNames = new []{ queueName };
        }

        public IQueueOptionalSettings WithQueueConfiguration(SqsConfiguration sqsConfiguration)
        {
            _sqsConfiguration = sqsConfiguration;
            return this;
        }

        public async Task<IEnumerable<string>> BuildAsync()
        {
            return await _queueProvider.CreateQueuesAsync(_queueNames, _sqsConfiguration ?? new SqsConfiguration()).ToListAsync();
        }
    }
}
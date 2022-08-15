using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Topica.Aws.Contracts;

namespace Topica.Aws.Queues
{
    public class AwsQueueOptionalSettings : IQueueOptionalSettings
    {
        private readonly IEnumerable<string> _queueNames;
        private readonly IQueueProvider _queueProvider;
        private QueueConfiguration? _queueConfiguration = null!;

        public AwsQueueOptionalSettings(IQueueProvider queueProvider, IEnumerable<string> queueNames)
        {
            _queueProvider = queueProvider;
            _queueNames = queueNames;
        }
        
        public AwsQueueOptionalSettings(IQueueProvider queueProvider, string queueName)
        {
            _queueProvider = queueProvider;
            _queueNames = new []{ queueName };
        }

        public IQueueOptionalSettings WithQueueConfiguration(QueueConfiguration? queueConfiguration)
        {
            _queueConfiguration = queueConfiguration;
            return this;
        }

        public async Task<IEnumerable<string>> BuildAsync()
        {
            return await _queueProvider.CreateQueuesAsync(_queueNames, _queueConfiguration ?? new QueueConfiguration()).ToListAsync();
        }
    }
}
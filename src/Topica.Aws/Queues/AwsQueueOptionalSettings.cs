using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Topica.Aws.Contracts;

namespace Topica.Aws.Queues
{
    public class AwsQueueOptionalSettings : IQueueOptionalSettings
    {
        private readonly IEnumerable<string> _queueNames;
        private readonly IAwsQueueService _awsQueueService;
        private QueueConfiguration? _queueConfiguration = null!;

        public AwsQueueOptionalSettings(IAwsQueueService awsQueueService, IEnumerable<string> queueNames)
        {
            _awsQueueService = awsQueueService;
            _queueNames = queueNames;
        }
        
        public AwsQueueOptionalSettings(IAwsQueueService awsQueueService, string queueName)
        {
            _awsQueueService = awsQueueService;
            _queueNames = new []{ queueName };
        }

        public IQueueOptionalSettings WithQueueConfiguration(QueueConfiguration? queueConfiguration)
        {
            _queueConfiguration = queueConfiguration;
            return this;
        }

        public async Task<IEnumerable<string>> BuildAsync()
        {
            return await _awsQueueService.CreateQueuesAsync(_queueNames, _queueConfiguration ?? new QueueConfiguration()).ToListAsync();
        }
    }
}
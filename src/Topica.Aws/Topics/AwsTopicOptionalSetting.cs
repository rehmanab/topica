using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Topica.Aws.Queues;

namespace Topica.Aws.Topics
{
    public class AwsTopicOptionalSetting : IAwsTopicOptionalSetting
    {
        private readonly string _topicName;
        private readonly ITopicProvider _topicProvider;

        private readonly IList<string> _queuesToAdd;
        private QueueConfiguration? _queueConfiguration;

        public AwsTopicOptionalSetting(string topicName, ITopicProvider topicProvider)
        {
            _topicName = topicName;
            _topicProvider = topicProvider;
            _queuesToAdd = new List<string>();
        }

        public IAwsTopicOptionalSetting WithSubscribedQueue(string queueName)
        {
            _queuesToAdd.Add(queueName);
            return this;
        }

        public IAwsTopicOptionalSetting WithSubscribedQueue(IEnumerable<string> queueNames)
        {
            queueNames.ToList().ForEach(x => _queuesToAdd.Add(x));
            return this;
        }

        public IAwsTopicOptionalSetting WithQueueConfiguration(QueueConfiguration? sqsConfiguration)
        {
            _queueConfiguration = sqsConfiguration;
            return this;
        }

        public async Task<string> BuildAsync()
        {
            return await _topicProvider.CreateTopicWithOptionalQueuesSubscribedAsync(_topicName, _queuesToAdd.ToArray(), _queueConfiguration);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Topica.Aws.Contracts;
using Topica.Aws.Queues;

namespace Topica.Aws.Topics
{
    public class AwsTopicOptionalSetting : IAwsTopicOptionalSetting
    {
        private readonly string _topicName;
        private readonly IAwsTopicService _awsTopicService;

        private readonly IList<string> _queuesToAdd;
        private SqsConfiguration? _sqsConfiguration;

        public AwsTopicOptionalSetting(string topicName, IAwsTopicService awsTopicService)
        {
            _topicName = topicName;
            _awsTopicService = awsTopicService;
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

        public IAwsTopicOptionalSetting WithSqsConfiguration(SqsConfiguration? sqsConfiguration)
        {
            _sqsConfiguration = sqsConfiguration;
            return this;
        }

        public async Task<string> BuildAsync()
        {
            return await _awsTopicService.CreateTopicWithOptionalQueuesSubscribedAsync(_topicName, _queuesToAdd.ToArray(), _sqsConfiguration);
        }
    }
}
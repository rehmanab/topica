using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Topica.Aws.Contracts;
using Topica.Aws.Queues;

namespace Topica.Aws.Topics
{
    public class AwsTopicOptionalSetting(string topicName, IAwsTopicService awsTopicService) : IAwsTopicOptionalSetting
    {
        private readonly List<string> _queuesToAdd = [];
        private AwsSqsConfiguration? _sqsConfiguration;

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

        public IAwsTopicOptionalSetting WithSqsConfiguration(AwsSqsConfiguration? sqsConfiguration)
        {
            _sqsConfiguration = sqsConfiguration;
            return this;
        }

        public async Task<string?> BuildAsync()
        {
            return await awsTopicService.CreateTopicWithOptionalQueuesSubscribedAsync(topicName, _queuesToAdd.ToArray(), _sqsConfiguration);
        }
    }
}
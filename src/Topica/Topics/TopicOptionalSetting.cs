using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Topica.Config;
using Topica.Contracts;

namespace Topica.Topics
{
    public interface ITopicOptionalSetting
    {
        ITopicOptionalSetting WithSubscribedQueue(string queueName);
        ITopicOptionalSetting WithSubscribedQueue(string[] queueNames);
        ITopicOptionalSetting WithQueueConfiguration(SqsConfiguration sqsConfiguration);
        Task<string> BuildAsync();
    }
    
    public class TopicOptionalSetting : ITopicOptionalSetting
    {
        private readonly string _topicName;
        private readonly ITopicProvider _topicProvider;

        private readonly IList<string> _queuesToAdd;
        private SqsConfiguration _sqsConfiguration;

        public TopicOptionalSetting(string topicName, ITopicProvider topicProvider)
        {
            _topicName = topicName;
            _topicProvider = topicProvider;
            _queuesToAdd = new List<string>();
        }

        public ITopicOptionalSetting WithSubscribedQueue(string queueName)
        {
            _queuesToAdd.Add(queueName);
            return this;
        }

        public ITopicOptionalSetting WithSubscribedQueue(string[] queueNames)
        {
            queueNames.ToList().ForEach(x => _queuesToAdd.Add(x));
            return this;
        }

        public ITopicOptionalSetting WithQueueConfiguration(SqsConfiguration sqsConfiguration)
        {
            _sqsConfiguration = sqsConfiguration;
            return this;
        }

        public async Task<string> BuildAsync()
        {
            return await _topicProvider.CreateTopicWithOptionalQueuesSubscribedAsync(_topicName, _queuesToAdd.ToArray(), _sqsConfiguration);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Aws.Messaging.Config;
using Aws.Messaging.Contracts;

namespace Aws.Messaging.Notifications
{
    public class TopicOptionalSetting : ITopicOptionalSetting
    {
        private readonly string _topicName;
        private readonly INotificationProvider _notificationProvider;

        private readonly IList<string> _queuesToAdd;
        private SqsConfiguration _sqsConfiguration;

        public TopicOptionalSetting(string topicName, INotificationProvider notificationProvider)
        {
            _topicName = topicName;
            _notificationProvider = notificationProvider;
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

        public string Create()
        {
            return _notificationProvider.CreateTopicWithOptionalQueuesSubscribedAsync(_topicName, _queuesToAdd.ToArray(), _sqsConfiguration).Result;
        }
    }
}
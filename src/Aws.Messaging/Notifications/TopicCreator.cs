using Aws.Messaging.Contracts;

namespace Aws.Messaging.Notifications
{
    public class TopicCreator : ITopicCreator
    {
        private readonly INotificationProvider _notificationProvider;

        public TopicCreator(INotificationProvider notificationProvider)
        {
            _notificationProvider = notificationProvider;
        }

        public ITopicOptionalSetting WithTopicName(string topicName)
        {
            return new TopicOptionalSetting(topicName, _notificationProvider); 
        }
    }
}
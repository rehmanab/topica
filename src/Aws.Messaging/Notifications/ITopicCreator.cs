using Aws.Messaging.Config;

namespace Aws.Messaging.Notifications
{
    public interface ITopicCreator
    {
        ITopicOptionalSetting WithTopicName(string topicName);
    }

    public interface ITopicOptionalSetting
    {
        ITopicOptionalSetting WithSubscribedQueue(string queueName);
        ITopicOptionalSetting WithSubscribedQueue(string[] queueNames);
        ITopicOptionalSetting WithQueueConfiguration(SqsConfiguration sqsConfiguration);
        string Create();
    }

    public interface ITopicComplete
    {
        
    }
}
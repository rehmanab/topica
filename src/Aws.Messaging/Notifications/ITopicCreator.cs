using System.Threading.Tasks;
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
        Task<string> CreateAsync();
    }

    public interface ITopicComplete
    {
        
    }
}
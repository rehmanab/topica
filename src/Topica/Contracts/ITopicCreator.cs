using System.Threading.Tasks;
using Topica.Topics;

namespace Topica.Contracts
{
    public interface ITopicCreator
    {
        MessagingPlatform MessagingPlatform { get; }
        Task<IConsumer> CreateTopic(TopicSettingsBase settings);
    }
}
using System.Threading.Tasks;
using Topica.Topics;

namespace Topica.Contracts
{
    public interface ITopicCreator
    {
        MessagingPlatform MessagingPlatform { get; }
        Task<string> CreateTopic(TopicConfigurationBase configuration);
    }
}
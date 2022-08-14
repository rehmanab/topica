using System.Threading.Tasks;
using Topica.Settings;

namespace Topica.Contracts
{
    public interface ITopicCreator
    {
        MessagingPlatform MessagingPlatform { get; }
        Task<IConsumer> CreateTopic(ConsumerSettings settings);
    }
}
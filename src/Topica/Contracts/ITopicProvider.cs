using System.Threading.Tasks;
using Topica.Settings;

namespace Topica.Contracts
{
    public interface ITopicProvider
    {
        MessagingPlatform MessagingPlatform { get; }
        
        Task CreateTopicAsync(ConsumerSettings settings);
        Task CreateTopicAsync(ProducerSettings settings);
    }
}
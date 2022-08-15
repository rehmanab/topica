using System.Threading.Tasks;
using Topica.Settings;

namespace Topica.Contracts
{
    public interface ITopicProvider
    {
        MessagingPlatform MessagingPlatform { get; }
        
        Task<IConsumer> CreateTopicAsync(ConsumerSettings settings);
        Task<IProducerBuilder> CreateTopicAsync(ProducerSettings settings);
        
        IConsumer GetConsumer();
        IProducerBuilder GetProducerBuilder();
    }
}
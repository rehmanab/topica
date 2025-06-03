using System.Threading.Tasks;
using Topica.Settings;

namespace Topica.Contracts
{
    public interface ITopicProvider
    {
        MessagingPlatform MessagingPlatform { get; }

        Task CreateTopicAsync(MessagingSettings settings);
        Task<IConsumer> ProvideConsumerAsync(MessagingSettings messagingSettings);
        Task<IProducer> ProvideProducerAsync(string producerName, MessagingSettings messagingSettings);
    }
}
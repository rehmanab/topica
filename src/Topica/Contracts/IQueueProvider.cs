using System.Threading.Tasks;
using Topica.Settings;

namespace Topica.Contracts;

public interface IQueueProvider
{
    MessagingPlatform MessagingPlatform { get; }

    Task CreateQueueAsync(MessagingSettings settings);
    Task<IConsumer> ProvideConsumerAsync(MessagingSettings messagingSettings);
    Task<IProducer> ProvideProducerAsync(string producerName, MessagingSettings messagingSettings);
}
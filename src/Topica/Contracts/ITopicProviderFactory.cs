namespace Topica.Contracts
{
    public interface ITopicProviderFactory
    {
        ITopicProvider Create(MessagingPlatform messagingPlatform);
    }
}
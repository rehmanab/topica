namespace Topica.Contracts;

public interface IQueueProviderFactory
{
    IQueueProvider Create(MessagingPlatform messagingPlatform);
}
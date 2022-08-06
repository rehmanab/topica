namespace Topica.Contracts
{
    public interface ITopicCreatorFactory
    {
        ITopicCreator Create(MessagingPlatform messagingPlatform);
    }
}
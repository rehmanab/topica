namespace Topica.Aws.Topics
{
    public interface ITopicBuilder
    {
        ITopicOptionalSetting WithTopicName(string topicName);
    }
}
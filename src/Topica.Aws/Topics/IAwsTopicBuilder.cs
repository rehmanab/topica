namespace Topica.Aws.Topics
{
    public interface IAwsTopicBuilder
    {
        IAwsTopicOptionalSetting WithTopicName(string topicName);
    }
}
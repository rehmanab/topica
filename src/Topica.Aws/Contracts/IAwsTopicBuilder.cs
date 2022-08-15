namespace Topica.Aws.Contracts
{
    public interface IAwsTopicBuilder
    {
        IAwsTopicOptionalSetting WithTopicName(string topicName);
    }
}
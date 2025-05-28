namespace Topica.Aws.Contracts
{
    public interface IAwsProviderTopicBuilder
    {
        IAwsTopicOptionalSetting WithTopicName(string topicName);
    }
}
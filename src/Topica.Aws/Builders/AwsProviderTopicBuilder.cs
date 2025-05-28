using Topica.Aws.Contracts;
using Topica.Aws.Topics;

namespace Topica.Aws.Builders
{
    public class AwsProviderTopicBuilder(IAwsTopicService awsTopicService) : IAwsProviderTopicBuilder
    {
        public IAwsTopicOptionalSetting WithTopicName(string topicName)
        {
            return new AwsTopicOptionalSetting(topicName, awsTopicService); 
        }
    }
}
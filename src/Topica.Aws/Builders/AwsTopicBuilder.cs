using Topica.Aws.Contracts;
using Topica.Aws.Topics;

namespace Topica.Aws.Builders
{
    public class AwsTopicBuilder : IAwsTopicBuilder
    {
        private readonly IAwsTopicService _awsTopicService;

        public AwsTopicBuilder(IAwsTopicService awsTopicService)
        {
            _awsTopicService = awsTopicService;
        }

        public IAwsTopicOptionalSetting WithTopicName(string topicName)
        {
            return new AwsTopicOptionalSetting(topicName, _awsTopicService); 
        }
    }
}
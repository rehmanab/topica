namespace Topica.Aws.Topics
{
    public class AwsAwsTopicBuilder : IAwsTopicBuilder
    {
        private readonly ITopicProvider _topicProvider;

        public AwsAwsTopicBuilder(ITopicProvider topicProvider)
        {
            _topicProvider = topicProvider;
        }

        public IAwsTopicOptionalSetting WithTopicName(string topicName)
        {
            return new AwsTopicOptionalSetting(topicName, _topicProvider); 
        }
    }
}
namespace Topica.Aws.Topics
{
    public class AwsTopicBuilder : IAwsTopicBuilder
    {
        private readonly ITopicProvider _topicProvider;

        public AwsTopicBuilder(ITopicProvider topicProvider)
        {
            _topicProvider = topicProvider;
        }

        public IAwsTopicOptionalSetting WithTopicName(string topicName)
        {
            return new AwsTopicOptionalSetting(topicName, _topicProvider); 
        }
    }
}
namespace Topica.Aws.Topics
{
    public class AwsTopicBuilder : ITopicBuilder
    {
        private readonly ITopicProvider _topicProvider;

        public AwsTopicBuilder(ITopicProvider topicProvider)
        {
            _topicProvider = topicProvider;
        }

        public ITopicOptionalSetting WithTopicName(string topicName)
        {
            return new TopicOptionalSetting(topicName, _topicProvider); 
        }
    }
}
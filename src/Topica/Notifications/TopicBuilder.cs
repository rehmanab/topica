using Topica.Contracts;

namespace Topica.Notifications
{
    public interface ITopicBuilder
    {
        ITopicOptionalSetting WithTopicName(string topicName);
    }
    
    public class TopicBuilder : ITopicBuilder
    {
        private readonly ITopicProvider _topicProvider;

        public TopicBuilder(ITopicProvider topicProvider)
        {
            _topicProvider = topicProvider;
        }

        public ITopicOptionalSetting WithTopicName(string topicName)
        {
            return new TopicOptionalSetting(topicName, _topicProvider); 
        }
    }
}
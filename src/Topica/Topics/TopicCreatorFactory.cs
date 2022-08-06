using System;
using System.Collections.Generic;
using System.Linq;
using Topica.Contracts;

namespace Topica.Topics
{
    public class TopicCreatorFactory : ITopicCreatorFactory
    {
        private readonly IEnumerable<ITopicCreator> _topicCreators;

        public TopicCreatorFactory(IEnumerable<ITopicCreator> topicCreators)
        {
            _topicCreators = topicCreators;
        }
        
        public ITopicCreator Create(MessagingPlatform messagingPlatform)
        {
            var topicCreator = _topicCreators.FirstOrDefault(x => x.MessagingPlatform == messagingPlatform);

            if (topicCreator == null)
            {
                throw new NotImplementedException($"Topic creator is not implemented for MessagingPlatform: {messagingPlatform}");
            }

            return topicCreator;
        }
    }
}
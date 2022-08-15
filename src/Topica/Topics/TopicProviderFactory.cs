using System;
using System.Collections.Generic;
using System.Linq;
using Topica.Contracts;

namespace Topica.Topics
{
    public class TopicProviderFactory : ITopicProviderFactory
    {
        private readonly IEnumerable<ITopicProvider> _topicProviders;

        public TopicProviderFactory(IEnumerable<ITopicProvider> topicProviders)
        {
            _topicProviders = topicProviders;
        }
        
        public ITopicProvider Create(MessagingPlatform messagingPlatform)
        {
            var topicProvider = _topicProviders.FirstOrDefault(x => x.MessagingPlatform == messagingPlatform);

            if (topicProvider == null)
            {
                throw new NotImplementedException($"Topic provider is not implemented for MessagingPlatform: {messagingPlatform}");
            }

            return topicProvider;
        }
    }
}
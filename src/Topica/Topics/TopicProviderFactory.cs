using System;
using System.Collections.Generic;
using System.Linq;
using Topica.Contracts;

namespace Topica.Topics
{
    public class TopicProviderFactory(IEnumerable<ITopicProvider> topicProviders) : ITopicProviderFactory
    {
        public ITopicProvider Create(MessagingPlatform messagingPlatform)
        {
            var topicProvider = topicProviders.FirstOrDefault(x => x.MessagingPlatform == messagingPlatform);

            if (topicProvider == null)
            {
                throw new NotImplementedException($"Topic provider is not implemented for MessagingPlatform: {messagingPlatform}");
            }

            return topicProvider;
        }
    }
}
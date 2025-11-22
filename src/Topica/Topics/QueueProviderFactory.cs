using System;
using System.Collections.Generic;
using System.Linq;
using Topica.Contracts;

namespace Topica.Topics;

public class QueueProviderFactory(IEnumerable<IQueueProvider> queueProviders) : IQueueProviderFactory
{
    public IQueueProvider Create(MessagingPlatform messagingPlatform)
    {
        var queueProvider = queueProviders.FirstOrDefault(x => x.MessagingPlatform == messagingPlatform);

        return queueProvider ?? throw new NotImplementedException($"Queue provider is not implemented for MessagingPlatform: {messagingPlatform}");
    }
}
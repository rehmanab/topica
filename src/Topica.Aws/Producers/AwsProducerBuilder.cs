using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Aws.Producers
{
    public class AwsProducerBuilder(ITopicProviderFactory topicProviderFactory, IAmazonSimpleNotificationService snsService) : IProducerBuilder, IDisposable
    {
        public async Task<T> BuildProducerAsync<T>(string producerName, ProducerSettings producerSettings, CancellationToken cancellationToken)
        {
            await topicProviderFactory.Create(MessagingPlatform.Aws).CreateTopicAsync(producerSettings);

            return await Task.FromResult((T)snsService);
        }

        public void Dispose()
        {
            snsService.Dispose();
        }
    }
}
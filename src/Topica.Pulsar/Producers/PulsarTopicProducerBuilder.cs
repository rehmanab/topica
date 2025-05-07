using System;
using System.Threading;
using System.Threading.Tasks;
using Pulsar.Client.Api;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Pulsar.Producers
{
    public class PulsarTopicProducerBuilder : IProducerBuilder, IDisposable
    {
        private readonly ITopicProviderFactory _topicProviderFactory;
        private readonly PulsarClientBuilder _clientBuilder;
        private IProducer<byte[]>? _producer;

        public PulsarTopicProducerBuilder(ITopicProviderFactory topicProviderFactory, PulsarClientBuilder clientBuilder)
        {
            _topicProviderFactory = topicProviderFactory;
            _clientBuilder = clientBuilder;
        }

        public async Task<T> BuildProducerAsync<T>(string producerName, ProducerSettings producerSettings, CancellationToken cancellationToken)
        {
            var topicProvider = _topicProviderFactory.Create(MessagingPlatform.Pulsar);
            await topicProvider.CreateTopicAsync(producerSettings);
            
            var client = await _clientBuilder.BuildAsync();
            _producer = await client.NewProducer(Schema.BYTES())
                .ProducerName(producerName)
                .Topic($"persistent://{producerSettings.PulsarTenant}/{producerSettings.PulsarNamespace}/{producerSettings.Source}")
                .BlockIfQueueFull(producerSettings.PulsarBlockIfQueueFull)
                .MaxPendingMessages(producerSettings.PulsarMaxPendingMessages < 0 ? int.MaxValue : producerSettings.PulsarMaxPendingMessages)
                .MaxPendingMessagesAcrossPartitions(producerSettings.PulsarMaxPendingMessagesAcrossPartitions < 0 ? int.MaxValue : producerSettings.PulsarMaxPendingMessagesAcrossPartitions)
                .EnableBatching(producerSettings.PulsarEnableBatching)
                .EnableChunking(producerSettings.PulsarEnableChunking) // Big messages are chuncked into smaller pieces
                .BatchingMaxMessages(producerSettings.PulsarBatchingMaxMessages) // Batch, consumer will only ack messages after consumer has read all messages in the batch
                .BatchingMaxPublishDelay(TimeSpan.FromMilliseconds(producerSettings.PulsarBatchingMaxPublishDelayMilliseconds)) // Will delay upto this value before sending batch. Have to wait at least this amount before disposing
                .CreateAsync();

            return (T)_producer;
        }

        public void Dispose()
        {
            _producer?.DisposeAsync().GetAwaiter().GetResult();
        }
    }
}
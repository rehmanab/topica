using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pulsar.Client.Api;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Pulsar.Consumers
{
    public class PulsarTopicProducerBuilder : IProducerBuilder, IDisposable
    {
        private readonly PulsarClientBuilder _clientBuilder;
        private readonly ILogger<PulsarTopicProducerBuilder> _logger;
        private IProducer<byte[]>? _producer;

        public PulsarTopicProducerBuilder(PulsarClientBuilder clientBuilder, ILogger<PulsarTopicProducerBuilder> logger)
        {
            _clientBuilder = clientBuilder;
            _logger = logger;
        }

        public async Task<T> BuildProducerAsync<T>(string producerName, ProducerSettings producerSettings, CancellationToken cancellationToken)
        {
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
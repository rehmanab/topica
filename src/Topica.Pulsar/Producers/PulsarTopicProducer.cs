using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pulsar.Client.Api;
using Topica.Contracts;
using Topica.Messages;
using Topica.Settings;

namespace Topica.Pulsar.Producers;

public class PulsarTopicProducer(string producerName, PulsarClientBuilder clientBuilder, MessagingSettings messagingSettings) : IProducer
{
    private IProducer<byte[]>? _producer;
    
    public async Task ProduceAsync(string source, BaseMessage message, Dictionary<string, string>? attributes = null, CancellationToken cancellationToken = default)
    {
        if (_producer == null)
        {
            var client = await clientBuilder.BuildAsync();
            _producer = await client.NewProducer(Schema.BYTES())
                .ProducerName(producerName)
                .Topic($"persistent://{messagingSettings.PulsarTenant}/{messagingSettings.PulsarNamespace}/{messagingSettings.Source}")
                .BlockIfQueueFull(messagingSettings.PulsarBlockIfQueueFull)
                .MaxPendingMessages(messagingSettings.PulsarMaxPendingMessages)
                .MaxPendingMessagesAcrossPartitions(messagingSettings.PulsarMaxPendingMessagesAcrossPartitions)
                .EnableBatching(messagingSettings.PulsarEnableBatching)
                .EnableChunking(messagingSettings.PulsarEnableChunking) // Big messages are chuncked into smaller pieces
                .BatchingMaxMessages(messagingSettings.PulsarBatchingMaxMessages) // Batch, consumer will only ack messages after consumer has read all messages in the batch
                .BatchingMaxPublishDelay(TimeSpan.FromMilliseconds(messagingSettings.PulsarBatchingMaxPublishDelayMilliseconds)) // Will delay upto this value before sending batch. Have to wait at least this amount before disposing
                .CreateAsync();
        }
        
        await _producer.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
    }

    public async Task FlushAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        // does not require explicit flushing, messages are sent immediately
        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if(_producer != null) await _producer.DisposeAsync();
    }
}
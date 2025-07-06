using System;
using System.Collections.Generic;
using System.Linq;
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
    
    public async Task ProduceAsync(string source, BaseMessage message, Dictionary<string, string>? attributes, CancellationToken cancellationToken)
    {
        var attributesToUse = attributes ?? new Dictionary<string, string>();
        attributesToUse.Add("ProducerName", producerName);
        
        // Pulsar doesn't have extra attributes that can be set (far as I know), so they are added to the message itself
        attributesToUse?.ToList().ForEach(x => message.MessageAdditionalProperties.TryAdd(x.Key, x.Value));
        
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

    public async Task ProduceBatchAsync(string source, IEnumerable<BaseMessage> messages, Dictionary<string, string>? attributes, CancellationToken cancellationToken)
    {
        var attributesToUse = attributes ?? new Dictionary<string, string>();
        attributesToUse.Add("ProducerName", producerName);
        
        if (_producer == null)
        {
            var client = await clientBuilder.BuildAsync();
            _producer = await client.NewProducer(Schema.BYTES())
                .ProducerName(producerName)
                .Topic($"persistent://{messagingSettings.PulsarTenant}/{messagingSettings.PulsarNamespace}/{messagingSettings.Source}")
                .BlockIfQueueFull(messagingSettings.PulsarBlockIfQueueFull)
                .MaxPendingMessages(messagingSettings.PulsarMaxPendingMessages)
                .MaxPendingMessagesAcrossPartitions(messagingSettings.PulsarMaxPendingMessagesAcrossPartitions)
                .EnableBatching(true)
                .EnableChunking(messagingSettings.PulsarEnableChunking) // Big messages are chuncked into smaller pieces
                .BatchingMaxMessages(5000) // Batch, consumer will only ack messages after consumer has read all messages in the batch
                .BatchingMaxPublishDelay(TimeSpan.FromMilliseconds(1000)) // Will delay upto this value before sending batch. Have to wait at least this amount before disposing
                .CreateAsync();
        }
        
        var tasks = new List<Task>();
        tasks.AddRange(messages.Select(x =>
        {
            x.MessageAdditionalProperties = attributesToUse;
            return _producer.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(x)));
        }));
        
        await Task.WhenAll(tasks);
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Topica.Contracts;
using Topica.Messages;
using Topica.Settings;

namespace Topica.RabbitMq.Producers;

internal class RabbitMqQueueProducer(string producerName, ConnectionFactory rabbitMqConnectionFactory, MessagingSettings messagingSettings) : IProducer
{
    private IChannel? _channel;
    
    public string Source => messagingSettings.Source;
    
    public async Task ProduceAsync(BaseMessage message, Dictionary<string, string>? attributes, CancellationToken cancellationToken)
    {
        var attributesToUse = attributes ?? new Dictionary<string, string>();
        attributesToUse.Add("ProducerName", producerName);
        
        if (_channel == null)
        {
            var connection = await rabbitMqConnectionFactory.CreateConnectionAsync(cancellationToken);
            _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        }

        var messageBodyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
        var props = new BasicProperties
        {
            Headers = attributesToUse.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value as object) ?? null)
        };
        await _channel.BasicPublishAsync(string.Empty, messagingSettings.Source, true, props, messageBodyBytes, cancellationToken);
    }

    public async Task ProduceBatchAsync(IEnumerable<BaseMessage> messages, Dictionary<string, string>? attributes, CancellationToken cancellationToken)
    {
        var attributesToUse = attributes ?? new Dictionary<string, string>();
        attributesToUse.Add("ProducerName", producerName);
        
        if (_channel == null)
        {
            var connection = await rabbitMqConnectionFactory.CreateConnectionAsync(cancellationToken);
            _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        }

        var tasks = new List<ValueTask>();
        tasks.AddRange(messages.Select(x =>
        {
            var messageBodyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(x));
            var props = new BasicProperties
            {
                Headers = attributesToUse.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value as object) ?? null)
            };
            return _channel.BasicPublishAsync(string.Empty, messagingSettings.Source, true, props, messageBodyBytes, cancellationToken);
        }));
        
        await Task.WhenAll(tasks.Select(x => x.AsTask()));
    }

    public async Task FlushAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        // does not require explicit flushing, messages are sent immediately
        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null) await _channel.DisposeAsync();
    }
}
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

namespace Topica.RabbitMq.Producers;

public class RabbitMqTopicProducer(ConnectionFactory rabbitMqConnectionFactory) : IProducer
{
    private IChannel? _channel;

    public async Task ProduceAsync(string source, BaseMessage message, Dictionary<string, string>? attributes, CancellationToken cancellationToken)
    {
        if (_channel == null)
        {
            var connection = await rabbitMqConnectionFactory.CreateConnectionAsync(cancellationToken);
            _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        }

        await _channel.BasicPublishAsync(source, "", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)), cancellationToken);
    }

    public async Task ProduceBatchAsync(string source, IEnumerable<BaseMessage> messages, Dictionary<string, string>? attributes, CancellationToken cancellationToken)
    {
        if (_channel == null)
        {
            var connection = await rabbitMqConnectionFactory.CreateConnectionAsync(cancellationToken);
            _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        }

        var tasks = new List<ValueTask>();
        tasks.AddRange(messages.Select(x => _channel.BasicPublishAsync(source, "", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(x)), cancellationToken)));
        
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
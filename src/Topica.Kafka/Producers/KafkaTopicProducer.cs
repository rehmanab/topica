using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Newtonsoft.Json;
using Topica.Contracts;
using Topica.Messages;
using Topica.Settings;

namespace Topica.Kafka.Producers;

public class KafkaTopicProducer(string consumerName, MessagingSettings messagingSettings) : IProducer
{
    private IProducer<string, string>? _producer;

    public async Task ProduceAsync(string source, BaseMessage message, Dictionary<string, string>? attributes, CancellationToken cancellationToken)
    {
        if (_producer is null)
        {
            var config = new ProducerConfig
            {
                // TransactionalId = consumerName, // Doesn't work, maybe need to set topic as transactional (Kafka throws "Erroneous state") error
                BootstrapServers = string.Join(",", messagingSettings.KafkaBootstrapServers),
                //SaslMechanism = SaslMechanism.Plain
                //SecurityProtocol = SecurityProtocol.Ssl
            };

            _producer = new ProducerBuilder<string, string>(config)
                //.SetValueSerializer(Serializers.ByteArray)
                .Build();
        }

        var result = await _producer.ProduceAsync(source, new Message<string, string>
        {
            Key = message.GetType().Name,
            Value = JsonConvert.SerializeObject(message)
        }, cancellationToken);
    }

    public async Task ProduceBatchAsync(string source, IEnumerable<BaseMessage> messages, Dictionary<string, string>? attributes, CancellationToken cancellationToken)
    {
        if (_producer is null)
        {
            var config = new ProducerConfig
            {
                // TransactionalId = consumerName, // Doesn't work, maybe need to set topic as transactional (Kafka throws "Erroneous state") error
                BootstrapServers = string.Join(",", messagingSettings.KafkaBootstrapServers),
                //SaslMechanism = SaslMechanism.Plain
                //SecurityProtocol = SecurityProtocol.Ssl
            };

            _producer = new ProducerBuilder<string, string>(config)
                //.SetValueSerializer(Serializers.ByteArray)
                .Build();
        }

        var tasks = new List<Task>();
        tasks.AddRange(messages.Select(x => _producer.ProduceAsync(source, new Message<string, string>
        {
            Key = x.GetType().Name,
            Value = JsonConvert.SerializeObject(x)
        }, cancellationToken)));
        
        await Task.WhenAll(tasks);
    }

    public async Task FlushAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        // Flush will wait for all messages sent to be confirmed/delivered, so don't use after each Produce()
        _producer?.Flush(timeout);
        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _producer?.Dispose();
        await Task.CompletedTask;
    }
}
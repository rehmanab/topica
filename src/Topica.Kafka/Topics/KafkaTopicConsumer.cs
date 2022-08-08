using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Kafka.Settings;
using Topica.Messages;
using Topica.Settings;

namespace Topica.Kafka.Topics
{
    public class KafkaTopicConsumer : IConsumer
    {
        private readonly KafkaSettings _kafkaSettings;
        private readonly ILogger<KafkaTopicConsumer> _logger;

        public KafkaTopicConsumer(KafkaSettings kafkaSettings, ILogger<KafkaTopicConsumer> logger)
        {
            _kafkaSettings = kafkaSettings;
            _logger = logger;
        }
        
        public async Task StartAsync<T>(string consumerName, ConsumerItemSettings consumerItemSettings, CancellationToken cancellationToken = default) where T : Message
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = string.Join(",", _kafkaSettings.BootstrapServers),
                GroupId = consumerItemSettings.ConsumerGroup,
                AutoOffsetReset = consumerItemSettings.StartFromEarliestMessages ? AutoOffsetReset.Earliest : AutoOffsetReset.Latest,
                SaslMechanism = SaslMechanism.Plain
                //SecurityProtocol = SecurityProtocol.Ssl
            };
            
            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            
            consumer.Subscribe(consumerItemSettings.Source);
            
            _logger.LogInformation($"Kafka: TopicConsumer Subscribed: {consumerItemSettings.Source}");

            await Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var msg = consumer.Consume();
                    _logger.LogInformation($"{msg.Message.Timestamp.UtcDateTime} : TopicConsumer: {consumerName} : {msg.TopicPartitionOffset} (topic [partition] @ offset): {msg.Message.Value}");
                    //await Task.Delay(500);
                }

                consumer.Dispose();
            }, cancellationToken);
        }
    }
}
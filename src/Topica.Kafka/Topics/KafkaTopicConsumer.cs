using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Topica.Contracts;
using Topica.Kafka.Settings;
using Topica.Messages;
using Topica.Settings;

namespace Topica.Kafka.Topics
{
    public class KafkaTopicConsumer : IConsumer
    {
        private readonly IMessageHandlerExecutor _messageHandlerExecutor;
        private readonly KafkaSettings _kafkaSettings;
        private readonly ILogger<KafkaTopicConsumer> _logger;

        public KafkaTopicConsumer(IMessageHandlerExecutor messageHandlerExecutor, KafkaSettings kafkaSettings, ILogger<KafkaTopicConsumer> logger)
        {
            _messageHandlerExecutor = messageHandlerExecutor;
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

            await Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var message = consumer.Consume();
                    
                    if (message == null)
                    {
                        throw new Exception($"{nameof(KafkaTopicConsumer)}: {consumerName} - Received null message on Topic: {consumerItemSettings.Source}");
                    }

                    var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync(typeof(T).Name, message.Message.Value);
                    _logger.LogDebug($"**** {nameof(KafkaTopicConsumer)}: TopicConsumer: {consumerName}: {handlerName} {(success ? "SUCCEEDED" : "FAILED")} ****");
                    _logger.LogDebug($"{message.Message.Timestamp.UtcDateTime} : TopicConsumer: {consumerName} : {message.TopicPartitionOffset} (topic [partition] @ offset): {message.Message.Value}");
                }

                consumer.Dispose();
            }, cancellationToken)
                .ContinueWith(x =>
                {
                    if (x.IsFaulted || x.Exception != null)
                    {
                        _logger.LogError(x.Exception, "{nameof(KafkaTopicConsumer)}: TopicConsumer: {consumerName}: Error");      
                    }
                }, cancellationToken);
        }
    }
}
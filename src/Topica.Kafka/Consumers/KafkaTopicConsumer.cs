using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Kafka.Consumers
{
    public class KafkaTopicConsumer : IConsumer
    {
        private readonly ITopicProviderFactory _topicProviderFactory;
        private readonly IMessageHandlerExecutor _messageHandlerExecutor;
        private readonly ILogger<KafkaTopicConsumer> _logger;

        public KafkaTopicConsumer(ITopicProviderFactory topicProviderFactory, IMessageHandlerExecutor messageHandlerExecutor, ILogger<KafkaTopicConsumer> logger)
        {
            _topicProviderFactory = topicProviderFactory;
            _messageHandlerExecutor = messageHandlerExecutor;
            _logger = logger;
        }

        public Task ConsumeAsync(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken)
        {
            Parallel.ForEach(Enumerable.Range(1, consumerSettings.NumberOfInstances), index =>
            {
                StartAsync($"{consumerName}-({index})", consumerSettings, cancellationToken);
            });
            
            return Task.CompletedTask;
        }

        public async Task StartAsync(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = string.Join(",", consumerSettings.KafkaBootstrapServers),
                GroupId = consumerSettings.KafkaConsumerGroup,
                AutoOffsetReset = consumerSettings.KafkaStartFromEarliestMessages ? AutoOffsetReset.Earliest : AutoOffsetReset.Latest,
                SaslMechanism = SaslMechanism.Plain
                //SecurityProtocol = SecurityProtocol.Ssl
            };

            await _topicProviderFactory.Create(MessagingPlatform.Kafka).CreateTopicAsync(consumerSettings);
            
            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            
            consumer.Subscribe(consumerSettings.Source);
            
            _logger.LogInformation("{KafkaTopicConsumerName}: Subscribed: {ConsumerSettingsSource}", nameof(KafkaTopicConsumer), consumerSettings.Source);

            await Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var message = consumer.Consume();
                    
                    if (message == null)
                    {
                        throw new Exception($"{nameof(KafkaTopicConsumer)}: {consumerName} - Received null message on Topic: {consumerSettings.Source}");
                    }

                    var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync(consumerSettings.MessageToHandle, message.Message.Value);
                    _logger.LogInformation($"**** {nameof(KafkaTopicConsumer)}: {consumerName}: {handlerName} {(success ? "SUCCEEDED" : "FAILED")} ****");
                    _logger.LogDebug($"{message.Message.Timestamp.UtcDateTime}: {consumerName} : {message.TopicPartitionOffset} (topic [partition] @ offset): {message.Message.Value}");
                }

                consumer.Dispose();
                _logger.LogInformation("{KafkaTopicConsumerName}: Disposed", nameof(KafkaTopicConsumer));

            }, cancellationToken)
                .ContinueWith(x =>
                {
                    if (x.IsFaulted || x.Exception != null)
                    {
                        _logger.LogError(x.Exception, "{ClassName}: {ConsumerName}: Error", nameof(KafkaTopicConsumer), consumerName);      
                    }
                }, cancellationToken);
        }
    }
}
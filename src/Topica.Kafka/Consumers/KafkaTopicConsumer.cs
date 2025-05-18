using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Kafka.Consumers
{
    public class KafkaTopicConsumer : IConsumer
    {
        private readonly ITopicProviderFactory _topicProviderFactory;
        private readonly IMessageHandlerExecutor _messageHandlerExecutor;
        private readonly ResiliencePipeline _retryPipeline;
        private readonly ILogger<KafkaTopicConsumer> _logger;

        public KafkaTopicConsumer(ITopicProviderFactory topicProviderFactory, IMessageHandlerExecutor messageHandlerExecutor, ILogger<KafkaTopicConsumer> logger)
        {
            _topicProviderFactory = topicProviderFactory;
            _messageHandlerExecutor = messageHandlerExecutor;
            _retryPipeline = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
            {
                BackoffType = DelayBackoffType.Constant,
                Delay = TimeSpan.FromSeconds(5),
                MaxRetryAttempts = int.MaxValue,
                OnRetry = args =>
                {
                    logger.LogWarning("Retrying: {ArgsAttemptNumber} in {RetryDelayTotalSeconds} seconds", args.AttemptNumber + 1, args.RetryDelay.TotalSeconds);
                    return default;
                }
            }).Build();
            _logger = logger;
        }

        public async Task ConsumeAsync<T>(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken) where T : IHandler
        {
            await _topicProviderFactory.Create(MessagingPlatform.Kafka).CreateTopicAsync(consumerSettings);

            Parallel.ForEach(Enumerable.Range(1, consumerSettings.NumberOfInstances), index =>
            {
                _retryPipeline.ExecuteAsync(x => StartAsync<T>($"{consumerName}-consumer-({index})", consumerSettings, x), cancellationToken);
            });
        }
        
        private async ValueTask StartAsync<T>(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken) where T : IHandler
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = string.Join(",", consumerSettings.KafkaBootstrapServers),
                GroupId = consumerSettings.KafkaConsumerGroup,
                AutoOffsetReset = consumerSettings.KafkaStartFromEarliestMessages
                    ? AutoOffsetReset.Earliest
                    : AutoOffsetReset.Latest,
                SaslMechanism = SaslMechanism.Plain
                //SecurityProtocol = SecurityProtocol.Ssl
            };

            try
            {
                var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
                consumer.Subscribe(consumerSettings.Source);

                _logger.LogInformation("{KafkaTopicConsumerName}: SUBSCRIBED TO: {ConsumerName}", nameof(KafkaTopicConsumer), consumerName);

                await Task.Run(async () =>
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var message = consumer.Consume(cancellationToken);

                            if (message == null)
                            {
                                throw new Exception($"{nameof(KafkaTopicConsumer)}: {consumerName} - Received null message on Topic: {consumerSettings.Source}");
                            }

                            var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync<T>(message.Message.Value);
                            // _logger.LogInformation("**** {KafkaTopicConsumerName}: {ConsumerName}: {HandlerName} {Succeeded} ****", nameof(KafkaTopicConsumer), consumerName, handlerName, success ? "SUCCEEDED" : "FAILED");
                            // _logger.LogDebug("{TimestampUtcDateTime}: {ConsumerName} : {MessageTopicPartitionOffset} (topic [partition] @ offset): {MessageValue}", message.Message.Timestamp.UtcDateTime, consumerName, message.TopicPartitionOffset, message.Message.Value);
                            
                            await Task.Delay(250, cancellationToken);
                        }

                        consumer.Dispose(); // Doesn't get here as consumer.Consume blocks
                        _logger.LogInformation("{KafkaTopicConsumerName}: Disposed", nameof(KafkaTopicConsumer));
                    }, cancellationToken)
                    .ContinueWith(x =>
                    {
                        if ((x.IsFaulted || x.Exception != null) && !x.IsCanceled)
                        {
                            _logger.LogError(x.Exception, "{ClassName}: {ConsumerName}: Error", nameof(KafkaTopicConsumer), consumerName);
                        }
                    }, cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "{ClassName}: {ConsumerName}: Error", nameof(KafkaTopicConsumer), consumerName);
                throw;
            }
        }
    }
}
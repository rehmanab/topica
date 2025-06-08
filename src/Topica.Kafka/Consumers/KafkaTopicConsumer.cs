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
        private readonly IMessageHandlerExecutor _messageHandlerExecutor;
        private readonly MessagingSettings _messagingSettings;
        private readonly ResiliencePipeline _retryPipeline;
        private readonly ILogger _logger;

        public KafkaTopicConsumer(IMessageHandlerExecutor messageHandlerExecutor, MessagingSettings messagingSettings, ILogger logger)
        {
            _messageHandlerExecutor = messageHandlerExecutor;
            _messagingSettings = messagingSettings;
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

        public async Task ConsumeAsync(CancellationToken cancellationToken)
        {
            Parallel.ForEach(Enumerable.Range(1, _messagingSettings.NumberOfInstances), index =>
            {
                _retryPipeline.ExecuteAsync(x => StartAsync($"{_messagingSettings.WorkerName}-({index})", _messagingSettings, x), cancellationToken);
            });
        }
        
        private async ValueTask StartAsync(string consumerName, MessagingSettings messagingSettings, CancellationToken cancellationToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = string.Join(",", messagingSettings.KafkaBootstrapServers!),
                
                // Each unique consumer group will will handle a share of the messages for that Topic
                // e.g. group1 with 10 conumers, share the messages
                // group2 will be like a new subscribed queue, and get all the messages
                // So each consumer GroupId is a subscribed queue
                // https://www.confluent.io/blog/configuring-apache-kafka-consumer-group-ids/
                GroupId = messagingSettings.KafkaConsumerGroup,
                AutoOffsetReset = messagingSettings.KafkaStartFromEarliestMessages
                    ? AutoOffsetReset.Earliest
                    : AutoOffsetReset.Latest,
                SaslMechanism = SaslMechanism.Plain
                //SecurityProtocol = SecurityProtocol.Ssl
            };

            try
            {
                var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
                consumer.Subscribe(messagingSettings.Source);

                _logger.LogInformation("**** STARTED CONSUMING: {ConsumerName}", consumerName);

                await Task.Run(async () =>
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var message = consumer.Consume(cancellationToken);

                            if (message == null)
                            {
                                throw new Exception($"{nameof(KafkaTopicConsumer)}: {consumerName} - Received null message on Topic: {messagingSettings.Source}");
                            }

                            var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync(message.Message.Value);
                            _logger.LogInformation("**** {KafkaTopicConsumerName}: {ConsumerName}: {HandlerName} {Succeeded} ****", nameof(KafkaTopicConsumer), consumerName, handlerName, success ? "SUCCEEDED" : "FAILED");
                            // _logger.LogDebug("{TimestampUtcDateTime}: {ConsumerName} : {MessageTopicPartitionOffset} (topic [partition] @ offset): {MessageValue}", message.Message.Timestamp.UtcDateTime, consumerName, message.TopicPartitionOffset, message.Message.Value);
                        }

                        consumer.Dispose(); // Doesn't get here as consumer.Consume blocks
                        _logger.LogInformation("**** Disposed: {KafkaTopicConsumerName}:", nameof(KafkaTopicConsumer));
                    }, cancellationToken)
                    .ContinueWith(x =>
                    {
                        if ((x.IsFaulted || x.Exception != null) && !x.IsCanceled)
                        {
                            _logger.LogError(x.Exception, "**** ERROR: {ClassName}: {ConsumerName}: Error", nameof(KafkaTopicConsumer), consumerName);
                        }
                    }, cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "**** ERROR: {ClassName}: {ConsumerName}: Error", nameof(KafkaTopicConsumer), consumerName);
                throw;
            }
        }
    }
}
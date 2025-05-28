using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Pulsar.Client.Api;
using Pulsar.Client.Common;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Pulsar.Consumers
{
    public class PulsarTopicConsumer : IConsumer
    {
        private readonly PulsarClientBuilder _clientBuilder;
        private readonly IMessageHandlerExecutor _messageHandlerExecutor;
        private readonly MessagingSettings _messagingSettings;
        private readonly ResiliencePipeline _retryPipeline;
        private readonly ILogger _logger;

        public PulsarTopicConsumer(PulsarClientBuilder clientBuilder, IMessageHandlerExecutor messageHandlerExecutor, MessagingSettings messagingSettings, ILogger logger)
        {
            _clientBuilder = clientBuilder;
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

        public async Task ConsumeAsync<T>(CancellationToken cancellationToken) where T : IHandler
        {
            Parallel.ForEach(Enumerable.Range(1, _messagingSettings.NumberOfInstances), index =>
            {
                _retryPipeline.ExecuteAsync(x => StartAsync<T>($"{typeof(T).Name}-consumer-({index})", _messagingSettings.PulsarConsumerGroup, _messagingSettings, x), cancellationToken);
            });
        }
        
        private async ValueTask StartAsync<T>(string consumerName, string consumerGroup, MessagingSettings messagingSettings, CancellationToken cancellationToken) where T : IHandler
        {
            try
            {
                var client = await _clientBuilder.BuildAsync();
                var consumer = await client.NewConsumer()
                    .Topic($"persistent://{messagingSettings.PulsarTenant}/{messagingSettings.PulsarNamespace}/{messagingSettings.Source}")
                    .ConsumerName(consumerName)
                    .SubscriptionName(consumerGroup) // will act as a new subscriber and read all messages if the name is unique
                    .SubscriptionType(SubscriptionType.Shared) // If the topic is partitioned, then shared will allow other concurrent consumers (scale horizontally), with the same subscription name to split the messages between them
                    .SubscriptionInitialPosition(messagingSettings.PulsarStartNewConsumerEarliest.HasValue && messagingSettings.PulsarStartNewConsumerEarliest.Value
                        ? SubscriptionInitialPosition.Earliest
                        : SubscriptionInitialPosition.Latest) //Earliest will read unread, Latest will read live incoming messages only
                    .SubscribeAsync();

                _logger.LogInformation("{PulsarTopicConsumerName}: Subscribed: {ConsumerSettingsSource}:{ConsumerSettingsPulsarConsumerGroup}", nameof(PulsarTopicConsumer), messagingSettings.Source, messagingSettings.PulsarConsumerGroup);

                await Task.Run(async () =>
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var message = await consumer.ReceiveAsync(cancellationToken);

                            if (message == null)
                            {
                                throw new Exception($"{nameof(PulsarTopicConsumer)}: {consumerName}:{messagingSettings.PulsarConsumerGroup} - Received null message on Topic: {messagingSettings.Source}");
                            }

                            var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync<T>(Encoding.UTF8.GetString(message.Data));
                            // _logger.LogInformation("**** {PulsarTopicConsumerName}: {ConsumerName}:{ConsumerSettingsPulsarConsumerGroup}: {HandlerName} {Succeeded} ****", nameof(PulsarTopicConsumer), consumerName, messagingSettings.PulsarConsumerGroup, handlerName, success ? "SUCCEEDED" : "FAILED");

                            if (success)
                            {
                                await consumer.AcknowledgeAsync(message.MessageId);
                            }
                        }

                        await consumer.DisposeAsync();
                        _logger.LogInformation("{PulsarTopicConsumerName}: Disposed", nameof(PulsarTopicConsumer));

                    }, cancellationToken)
                    .ContinueWith(x =>
                    {
                        if ((x.IsFaulted || x.Exception != null) && !x.IsCanceled)
                        {
                            _logger.LogError(x.Exception, "{ClassName}: {ConsumerName}: Error", nameof(PulsarTopicConsumer), $"{consumerName}:{messagingSettings.PulsarConsumerGroup}");
                        }
                    }, cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "{ClassName}: {ConsumerName}: Error", nameof(PulsarTopicConsumer), consumerName);
                throw;
            }
        }
    }
}
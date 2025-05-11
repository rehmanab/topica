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
        private readonly ITopicProviderFactory _topicProviderFactory;
        private readonly PulsarClientBuilder _clientBuilder;
        private readonly IMessageHandlerExecutor _messageHandlerExecutor;
        private readonly ResiliencePipeline _retryPipeline;
        private readonly ILogger<PulsarTopicConsumer> _logger;

        public PulsarTopicConsumer(ITopicProviderFactory topicProviderFactory, PulsarClientBuilder clientBuilder, IMessageHandlerExecutor messageHandlerExecutor, ILogger<PulsarTopicConsumer> logger)
        {
            _topicProviderFactory = topicProviderFactory;
            _clientBuilder = clientBuilder;
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

        public Task ConsumeAsync<T>(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken) where T : IHandler
        {
            throw new NotImplementedException();
        }

        public Task ConsumeAsync(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken)
        {
            Parallel.ForEach(Enumerable.Range(1, consumerSettings.NumberOfInstances), index =>
            {
                _retryPipeline.ExecuteAsync(x => StartAsync($"{consumerName}-({index})", consumerSettings, x), cancellationToken);
            });

            return Task.CompletedTask;
        }

        private async ValueTask StartAsync(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken)
        {
            try
            {
                await _topicProviderFactory.Create(MessagingPlatform.Pulsar).CreateTopicAsync(consumerSettings);

                var client = await _clientBuilder.BuildAsync();
                var consumer = await client.NewConsumer()
                    .Topic(
                        $"persistent://{consumerSettings.PulsarTenant}/{consumerSettings.PulsarNamespace}/{consumerSettings.Source}")
                    .SubscriptionName(consumerSettings.PulsarConsumerGroup)
                    .SubscriptionInitialPosition(consumerSettings.PulsarStartNewConsumerEarliest
                        ? SubscriptionInitialPosition.Earliest
                        : SubscriptionInitialPosition
                            .Latest) //Earliest will read unread, Latest will read live incoming messages only
                    .SubscribeAsync();

                _logger.LogInformation("{PulsarTopicConsumerName}: Subscribed: {ConsumerSettingsSource}:{ConsumerSettingsPulsarConsumerGroup}", nameof(PulsarTopicConsumer), consumerSettings.Source, consumerSettings.PulsarConsumerGroup);

                await Task.Run(async () =>
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var message = await consumer.ReceiveAsync(cancellationToken);

                            if (message == null)
                            {
                                throw new Exception($"{nameof(PulsarTopicConsumer)}: {consumerName}:{consumerSettings.PulsarConsumerGroup} - Received null message on Topic: {consumerSettings.Source}");
                            }

                            var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync(consumerSettings.MessageToHandle, Encoding.UTF8.GetString(message.Data));
                            _logger.LogInformation(
                                "**** {PulsarTopicConsumerName}: {ConsumerName}:{ConsumerSettingsPulsarConsumerGroup}: {HandlerName} {Succeeded} ****", nameof(PulsarTopicConsumer), consumerName, consumerSettings.PulsarConsumerGroup, handlerName, success ? "SUCCEEDED" : "FAILED");

                            if (success)
                            {
                                await consumer.AcknowledgeAsync(message.MessageId);
                            }
                        }

                    }, cancellationToken)
                    .ContinueWith(x =>
                    {
                        if (x.IsFaulted || x.Exception != null)
                        {
                            _logger.LogError(x.Exception, "{ClassName}: {ConsumerName}: Error", nameof(PulsarTopicConsumer), $"{consumerName}:{consumerSettings.PulsarConsumerGroup}");
                        }
                    }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}: {ConsumerName}: Error", nameof(PulsarTopicConsumer), consumerName);
                throw;
            }
        }
    }
}
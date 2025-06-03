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

        public async Task ConsumeAsync(CancellationToken cancellationToken)
        {
            Parallel.ForEach(Enumerable.Range(1, _messagingSettings.NumberOfInstances), index =>
            {
                _retryPipeline.ExecuteAsync(x => StartAsync($"{_messagingSettings.WorkerName}-({index})", x), cancellationToken);
            });
        }
        
        private async ValueTask StartAsync(string consumerName, CancellationToken cancellationToken)
        {
            try
            {
                var client = await _clientBuilder.BuildAsync();
                var consumer = await client.NewConsumer()
                    .Topic($"persistent://{_messagingSettings.PulsarTenant}/{_messagingSettings.PulsarNamespace}/{_messagingSettings.Source}")
                    .ConsumerName(consumerName)
                    .SubscriptionName(_messagingSettings.PulsarConsumerGroup) // will act as a new subscriber and read all messages if the name is unique
                    .SubscriptionType(SubscriptionType.Shared) // If the topic is partitioned, then shared will allow other concurrent consumers (scale horizontally), with the same subscription name to split the messages between them
                    .AcknowledgementsGroupTime(TimeSpan.FromSeconds(5)) // Acknowledgements will be sent immediately after processing the message, no batching, set this higher if you want to batch acknowledgements and less network traffic
                    .SubscriptionInitialPosition(_messagingSettings.PulsarStartNewConsumerEarliest
                        ? SubscriptionInitialPosition.Earliest
                        : SubscriptionInitialPosition.Latest) //Earliest will read unread, Latest will read live incoming messages only
                    .SubscribeAsync();

                _logger.LogInformation("{PulsarTopicConsumerName}: Subscribed: {ConsumerSettingsSource}:{ConsumerSettingsPulsarConsumerGroup}", nameof(PulsarTopicConsumer), _messagingSettings.Source, _messagingSettings.PulsarConsumerGroup);

                await Task.Run(async () =>
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var message = await consumer.ReceiveAsync(cancellationToken);

                            if (message == null)
                            {
                                throw new Exception($"{nameof(PulsarTopicConsumer)}: {consumerName}:{_messagingSettings.PulsarConsumerGroup} - Received null message on Topic: {_messagingSettings.Source}");
                            }

                            var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync(Encoding.UTF8.GetString(message.Data));

                            // Need to Acknowledge all the messages whether success or not, I think when using partitioned topics, else the messages are sent again after restarting the worker..
                            await consumer.AcknowledgeAsync(message.MessageId);

                            _logger.LogInformation("**** {PulsarTopicConsumerName}: {ConsumerName}:{ConsumerSettingsPulsarConsumerGroup}: {HandlerName} {Succeeded} ****", nameof(PulsarTopicConsumer), consumerName, _messagingSettings.PulsarConsumerGroup, handlerName, success ? "SUCCEEDED" : "FAILED");
                        }

                        await consumer.DisposeAsync();
                        _logger.LogInformation("{PulsarTopicConsumerName}: Disposed", nameof(PulsarTopicConsumer));

                    }, cancellationToken)
                    .ContinueWith(x =>
                    {
                        if ((x.IsFaulted || x.Exception != null) && !x.IsCanceled)
                        {
                            _logger.LogError(x.Exception, "{ClassName}: {ConsumerName}: Error", nameof(PulsarTopicConsumer), $"{consumerName}:{_messagingSettings.PulsarConsumerGroup}");
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
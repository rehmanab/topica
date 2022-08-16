using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<PulsarTopicConsumer> _logger;

        public PulsarTopicConsumer(PulsarClientBuilder clientBuilder, IMessageHandlerExecutor messageHandlerExecutor, ILogger<PulsarTopicConsumer> logger)
        {
            _clientBuilder = clientBuilder;
            _messageHandlerExecutor = messageHandlerExecutor;
            _logger = logger;
        }

        public Task ConsumeAsync(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken)
        {
            Parallel.ForEach(Enumerable.Range(1, consumerSettings.NumberOfInstances), instanceIndex =>
            {
                StartAsync($"{consumerName}", instanceIndex, consumerSettings, cancellationToken);
            });

            return Task.CompletedTask;
        }

        private async Task StartAsync(string consumerName, int instanceIndex, ConsumerSettings consumerSettings, CancellationToken cancellationToken)
        {
            var client = await _clientBuilder.BuildAsync();
            var consumer = await client.NewConsumer()
                .Topic($"persistent://{consumerSettings.PulsarTenant}/{consumerSettings.PulsarNamespace}/{consumerSettings.Source}")
                .SubscriptionName(consumerSettings.PulsarConsumerGroup)
                .SubscriptionInitialPosition(consumerSettings.PulsarStartNewConsumerEarliest ? SubscriptionInitialPosition.Earliest : SubscriptionInitialPosition.Latest) //Earliest will read unread, Latest will read live incoming messages only
                .SubscribeAsync();

            _logger.LogInformation($"{nameof(PulsarTopicConsumer)}: Subscribed: {consumerSettings.Source}:{consumerSettings.PulsarConsumerGroup}");

            await Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var message = await consumer.ReceiveAsync(cancellationToken);

                    if (message == null)
                    {
                        throw new Exception($"{nameof(PulsarTopicConsumer)}: {consumerName}-({instanceIndex}):{consumerSettings.PulsarConsumerGroup} - Received null message on Topic: {consumerSettings.Source}");
                    }

                    var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync(consumerSettings.MessageToHandle, Encoding.UTF8.GetString(message.Data));
                    _logger.LogInformation($"**** {nameof(PulsarTopicConsumer)}: {consumerName}-({instanceIndex}):{consumerSettings.PulsarConsumerGroup}: {handlerName} {(success ? "SUCCEEDED" : "FAILED")} ****");

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
                    _logger.LogError(x.Exception, "{ClassName}: {ConsumerName}: Error", nameof(PulsarTopicConsumer), $"{consumerName}-({instanceIndex}):{consumerSettings.PulsarConsumerGroup}");
                }
            }, cancellationToken);
        }
    }
}
using Azure.ServiceBus.Producer.Host.Messages.V1;
using Azure.ServiceBus.Producer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Azure.ServiceBus.Contracts;

namespace Azure.ServiceBus.Producer.Host;

public class Worker(IAzureServiceBusTopicFluentBuilder builder, AzureServiceBusProducerSettings settings, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const string workerName = $"{nameof(PriceSubmittedMessageV1)}_azure_service_bus_producer_host_1";
        
        var priceSubmittedProducer = await builder
            .WithWorkerName(workerName)
            .WithTopicName(settings.PriceSubmittedTopicSettings.Source)
            .WithSubscriptions(settings.PriceSubmittedTopicSettings.Subscriptions)
            .WithTimings
            (
                settings.PriceSubmittedTopicSettings.AutoDeleteOnIdle, 
                settings.PriceSubmittedTopicSettings.DefaultMessageTimeToLive, 
                settings.PriceSubmittedTopicSettings.DuplicateDetectionHistoryTimeWindow
            )
            .WithOptions
            (
                settings.PriceSubmittedTopicSettings.EnableBatchedOperations,
                settings.PriceSubmittedTopicSettings.EnablePartitioning,
                settings.PriceSubmittedTopicSettings.MaxSizeInMegabytes,
                settings.PriceSubmittedTopicSettings.RequiresDuplicateDetection,
                settings.PriceSubmittedTopicSettings.MaxMessageSizeInKilobytes,
                settings.PriceSubmittedTopicSettings.EnabledStatus,
                settings.PriceSubmittedTopicSettings.SupportOrdering
            )
            .WithMetadata(settings.PriceSubmittedTopicSettings.UserMetadata)
            .BuildProducerAsync(stoppingToken);
        

        var count = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = new PriceSubmittedMessageV1
            {
                PriceId = count,
                PriceName = "Some Price",
                ConversationId = Guid.NewGuid(),
                Type = nameof(PriceSubmittedMessageV1),
                RaisingComponent = workerName,
                Version = "V1",
                AdditionalProperties = new Dictionary<string, string> { { "prop1", "value1" } }
            };
            
            await priceSubmittedProducer.ProduceAsync(settings.PriceSubmittedTopicSettings.Source, message, null, stoppingToken);
            logger.LogInformation("Sent to {Topic}: {Count}", settings.PriceSubmittedTopicSettings.Source, count);
            count++;

            await Task.Delay(1000, stoppingToken);
        }
    }
}
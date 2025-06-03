using Topica.Host.Shared.Messages.V1;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMq.Producer.Host.Settings;
using Topica.RabbitMq.Contracts;

namespace RabbitMq.Producer.Host;

public class Worker(IRabbitMqTopicFluentBuilder builder, RabbitMqProducerSettings settings, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var producer1 = await builder
            .WithWorkerName(settings.WebAnalyticsTopicSettings.WorkerName)
            .WithTopicName(settings.WebAnalyticsTopicSettings.Source)
            .WithSubscribedQueues(
                settings.WebAnalyticsTopicSettings.SubscribeToSource,
                settings.WebAnalyticsTopicSettings.WithSubscribedQueues
            )
            .BuildProducerAsync(stoppingToken);

        var count = 1;
        while(!stoppingToken.IsCancellationRequested)
        {
            var message = new SearchTriggeredMessageV1 { ConversationId = Guid.NewGuid(), EventId = count, EventName = "search.triggered.web.v1", Type = nameof(SearchTriggeredMessageV1) };
            
            await producer1.ProduceAsync(settings.WebAnalyticsTopicSettings.Source, message, cancellationToken: stoppingToken);

            logger.LogInformation("Produced message to {MessagingSettingsSource}: {MessageIdName}", settings.WebAnalyticsTopicSettings.Source, $"{message.EventId} : {message.EventName}");
            count++;
            
            await Task.Delay(1000, stoppingToken);
        }

        await producer1.DisposeAsync();

        logger.LogInformation("Finished!");
    }
}
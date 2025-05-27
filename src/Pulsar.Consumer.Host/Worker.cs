using Microsoft.Extensions.Hosting;
using Pulsar.Consumer.Host.Handlers.V1;
using Pulsar.Consumer.Host.Messages.V1;
using Topica.Pulsar.Contracts;
using Topica.Pulsar.Settings;

namespace Pulsar.Consumer.Host;

public class Worker(IPulsarConsumerTopicFluentBuilder builder, PulsarConsumerSettings settings) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if(settings.DataSentTopicSettings is null)
        {
            throw new ApplicationException($"{nameof(settings.DataSentTopicSettings)} cannot be null.");
        }
        
        if(settings.MatchStartedTopicSettings is null)
        {
            throw new ApplicationException($"{nameof(settings.MatchStartedTopicSettings)} cannot be null.");
        }
        
        await builder
            .WithConsumerName(nameof(DataSentMessageV1))
            .WithTopicName(settings.DataSentTopicSettings.Source!)
            .WithConsumerGroup(settings.DataSentTopicSettings!.ConsumerGroup!)
            .WithConfiguration(settings.DataSentTopicSettings!.Tenant!, settings.DataSentTopicSettings!.Namespace!)
            .WithTopicOptions(settings.DataSentTopicSettings.StartNewConsumerEarliest)
            .StartConsumingAsync<DataSentMessageHandlerV1>(
                settings.DataSentTopicSettings.NumberOfInstances, 
                stoppingToken
            );
        
        await builder
            .WithConsumerName(nameof(MatchStartedMessageV1))
            .WithTopicName(settings.MatchStartedTopicSettings!.Source!)
            .WithConsumerGroup(settings.MatchStartedTopicSettings!.ConsumerGroup!)
            .WithConfiguration(settings.MatchStartedTopicSettings!.Tenant!, settings.MatchStartedTopicSettings!.Namespace!)
            .WithTopicOptions(settings.MatchStartedTopicSettings.StartNewConsumerEarliest)
            .StartConsumingAsync<MatchStartedMessageHandlerV1>(
                settings.MatchStartedTopicSettings.NumberOfInstances, 
                stoppingToken
            );
    }
}
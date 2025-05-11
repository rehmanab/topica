using System.Reflection;
using Aws.Consumer.Host.Handlers.V1;
using Aws.Consumer.Host.Messages.V1;
using Microsoft.Extensions.Hosting;
using Topica.Contracts;
using Topica.Settings;

namespace Aws.Consumer.Host;

public class Worker(IConsumer consumer, IConsumerFluentBuilder consumerFluentBuilder, IEnumerable<ConsumerSettings> consumerSettingsList) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var consumerSetting in consumerSettingsList)
        {
            var consumerName = $"{Assembly.GetExecutingAssembly().GetName().Name}-{consumerSetting.MessageToHandle}";
            
            await consumer.ConsumeAsync(consumerName, consumerSetting, stoppingToken);
            
            // await consumerFluentBuilder
            //     .WithConsumerName(consumerName)
            //     .WithTopicName(consumerSetting.Source)
            //     .WithSubscribedQueues(consumerSetting.WithSubscribedQueues)
            //     .StartConsumingAsync<OrderCreatedMessageHandler>(stoppingToken);
        }
    }
}
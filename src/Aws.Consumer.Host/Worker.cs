using System.Reflection;
using Aws.Consumer.Host.Handlers.V1;
using Microsoft.Extensions.Hosting;
using Topica.Aws.Contracts;
using Topica.Contracts;
using Topica.Settings;

namespace Aws.Consumer.Host;

public class Worker(IConsumer consumer, IAwsConsumerTopicFluentBuilder awsConsumerTopicFluentBuilder, IEnumerable<ConsumerSettings> consumerSettingsList) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var consumerSetting in consumerSettingsList)
        {
            var consumerName = $"{Assembly.GetExecutingAssembly().GetName().Name}-{consumerSetting.MessageToHandle}";
            
            // await consumer.ConsumeAsync(consumerName, consumerSetting, stoppingToken);
            
            await awsConsumerTopicFluentBuilder
                .WithConsumerName(consumerName)
                .WithTopicName(consumerSetting.Source)
                .WithSubscribedQueues(true, consumerSetting.WithSubscribedQueues)
                .WithFifoSettings(true, true)
                .StartConsumingAsync<OrderCreatedMessageHandler>(consumerSetting.NumberOfInstances, consumerSetting.AwsReceiveMaximumNumberOfMessages, stoppingToken);
        }
    }
}
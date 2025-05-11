using System.Reflection;
using Microsoft.Extensions.Hosting;
using Topica.Contracts;
using Topica.Settings;

namespace Aws.Consumer.Host;

public class Worker(IConsumer consumer, IEnumerable<ConsumerSettings> consumerSettingsList) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var consumerSetting in consumerSettingsList)
        {
            var consumerName = $"{Assembly.GetExecutingAssembly().GetName().Name}-{consumerSetting.MessageToHandle}";
            await consumer.ConsumeAsync(consumerName, consumerSetting, stoppingToken);
        }
    }
}
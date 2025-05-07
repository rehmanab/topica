using System.Reflection;
using Microsoft.Extensions.Hosting;
using Topica.Contracts;
using Topica.Settings;

namespace Kafka.Consumer.Host;

public class Worker(IConsumer consumer, IEnumerable<ConsumerSettings> consumerSettingsList) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var consumerSettings in consumerSettingsList)
        {
            var consumerName = $"{Assembly.GetExecutingAssembly().GetName().Name}-{consumerSettings.MessageToHandle}";
            await consumer.ConsumeAsync(consumerName, consumerSettings, stoppingToken);
        }
    }
}
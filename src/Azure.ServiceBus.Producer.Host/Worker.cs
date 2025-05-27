using System.Text;
using Azure.Messaging.ServiceBus;
using Azure.ServiceBus.Producer.Host.Messages.V1;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Topica.Azure.ServiceBus.Contracts;
using Topica.Azure.ServiceBus.Settings;
using Topica.Contracts;
using Topica.Settings;

namespace Azure.ServiceBus.Producer.Host;

public class Worker(IProducerBuilder producerBuilder, AzureServiceBusHostSettings hostSettings, ProducerSettings producerSettings, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const string consumerName = "azure_service_bus_producer_host_1";
        const string topicName = "ar_price_submitted_v1";
        
        producerSettings.ConnectionString = hostSettings.ConnectionString;
        var producer = await producerBuilder.BuildProducerAsync<IServiceBusClientProvider>(consumerName, producerSettings, stoppingToken);
        var sender = producer.Client.CreateSender(topicName, new ServiceBusSenderOptions { Identifier = consumerName });

        var theMessage = JsonConvert.SerializeObject(new PriceSubmittedMessageV1
        {
            PriceId = 1234L,
            PriceName = "Some Price",
            ConversationId = Guid.NewGuid(),
            Type = nameof(PriceSubmittedMessageV1),
            RaisingComponent = consumerName,
            Version = "V1",
            AdditionalProperties = new Dictionary<string, string> { { "prop1", "value1" } }
        });

        var count = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(theMessage))
            {
                MessageId = Guid.NewGuid().ToString() // MessageId is or can be used for deduplication
            };
            //message.ApplicationProperties.Add("userProp1", "value1");

            await sender.SendMessageAsync(message, stoppingToken);
            logger.LogInformation("Sent: {Count}", count);
            count++;

            await Task.Delay(1000, stoppingToken);
        }
    }
}
using System.Text;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Topica.Azure.ServiceBus.Contracts;
using Topica.Contracts;
using Topica.Messages;
using Topica.Settings;

namespace Topica.Azure.ServiceBus.Producers;

internal class AzureServiceBusTopicProducer(string producerName, IAzureServiceBusClientProvider provider, MessagingSettings messagingSettings) : IProducer
{
    private ServiceBusSender? _sender;

    public string Source => messagingSettings.Source;

    public async Task ProduceAsync(BaseMessage message, Dictionary<string, string>? attributes, CancellationToken cancellationToken)
    {
        _sender ??= provider.Client.CreateSender(messagingSettings.Source, new ServiceBusSenderOptions { Identifier = producerName });
        
        var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)))
        {
            MessageId = Guid.NewGuid().ToString() // MessageId is or can be used for deduplication
        };

        serviceBusMessage.ApplicationProperties.Add("ProducerName", producerName);
        attributes?.ToList().ForEach(x => serviceBusMessage.ApplicationProperties.Add(x.Key, x.Value));

        await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }

    public async Task ProduceBatchAsync(IEnumerable<BaseMessage> messages, Dictionary<string, string>? attributes, CancellationToken cancellationToken)
    {
        _sender ??= provider.Client.CreateSender(messagingSettings.Source, new ServiceBusSenderOptions { Identifier = producerName });
        using var messageBatch = await _sender.CreateMessageBatchAsync(cancellationToken);

        foreach (var message in messages)
        {
            var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)))
            {
                MessageId = Guid.NewGuid().ToString() // MessageId is or can be used for deduplication
            };
            
            serviceBusMessage.ApplicationProperties.Add("ProducerName", producerName);
            attributes?.ToList().ForEach(x => serviceBusMessage.ApplicationProperties.Add(x.Key, x.Value));
            
            messageBatch.TryAddMessage(serviceBusMessage);
        }

        await _sender.SendMessagesAsync(messageBatch, cancellationToken);
    }

    public async Task FlushAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        // does not require explicit flushing, messages are sent immediately
        await Task.CompletedTask;
    }
}
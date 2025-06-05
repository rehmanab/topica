using System.Text;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Topica.Azure.ServiceBus.Contracts;
using Topica.Contracts;
using Topica.Messages;

namespace Topica.Azure.ServiceBus.Producers;

public class AzureServiceBusTopicProducer(string producerName, IAzureServiceBusClientProvider provider) : IProducer
{
    private ServiceBusSender? _sender;

    public async Task ProduceAsync(string source, BaseMessage message, Dictionary<string, string>? attributes = null, CancellationToken cancellationToken = default)
    {
        _sender ??= provider.Client.CreateSender(source, new ServiceBusSenderOptions { Identifier = producerName });
        
        var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)))
        {
            MessageId = Guid.NewGuid().ToString() // MessageId is or can be used for deduplication
        };

        attributes?.ToList().ForEach(x => serviceBusMessage.ApplicationProperties.Add(x.Key, x.Value));

        await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }

    public async Task ProduceBatchAsync(string source, IEnumerable<BaseMessage> messages, Dictionary<string, string>? attributes = null, CancellationToken cancellationToken = default)
    {
        _sender ??= provider.Client.CreateSender(source, new ServiceBusSenderOptions { Identifier = producerName });
        using var messageBatch = await _sender.CreateMessageBatchAsync(cancellationToken);

        foreach (var message in messages)
        {
            var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)))
            {
                MessageId = Guid.NewGuid().ToString() // MessageId is or can be used for deduplication
            };
            serviceBusMessage.ApplicationProperties.Add("userProp1", "value1");
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

    public async ValueTask DisposeAsync()
    {
        if (_sender != null) await _sender.DisposeAsync();
        await provider.Client.DisposeAsync();
    }
}
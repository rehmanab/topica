using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Newtonsoft.Json;
using Topica.Contracts;
using Topica.Messages;

namespace Topica.Aws.Producer;

public class AwsTopicProducer(string producerName, IAmazonSimpleNotificationService? snsClient) : IProducer, IAsyncDisposable
{
    public async Task ProduceAsync(string source, BaseMessage message, Dictionary<string, string>? attributes = null, CancellationToken cancellationToken = default)
    {
        var request = new PublishRequest
        {
            TopicArn = source,
            Message = JsonConvert.SerializeObject(message)
        };
        
        if (attributes != null)
        {
            request.MessageAttributes = attributes.Select(x => new KeyValuePair<string, MessageAttributeValue>(
                x.Key,
                new MessageAttributeValue
                {
                    StringValue = x.Value,
                    DataType = "String"
                })).ToDictionary(x => x.Key, x => x.Value);
        }
        
        request.MessageAttributes.Add("ProducerName", new MessageAttributeValue
        {
            StringValue = producerName,
            DataType = "String"
        });

        if (source.EndsWith(".fifo"))
        {
            request.MessageGroupId = message.MessageGroupId; // TODO - should be the same for all messages in a group for it to be first in first out
            request.MessageDeduplicationId = Guid.NewGuid().ToString();
        }

        if (snsClient != null)
        {
            await snsClient.PublishAsync(request, cancellationToken);
        }
    }

    public async Task FlushAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        // does not require explicit flushing, messages are sent immediately
        await Task.CompletedTask;
    }

    ValueTask IProducer.DisposeAsync()
    {
        // No resources to dispose of in this implementation
        snsClient?.Dispose();
        return new ValueTask(Task.CompletedTask);
    }

    public async ValueTask DisposeAsync()
    {
        snsClient?.Dispose();
        await Task.CompletedTask;
    }
}
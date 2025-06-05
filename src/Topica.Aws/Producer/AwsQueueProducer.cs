using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using Topica.Aws.Helpers;
using Topica.Contracts;
using Topica.Helpers;
using Topica.Messages;

namespace Topica.Aws.Producer;

public class AwsQueueProducer(string producerName, IAmazonSQS? sqsClient) : IProducer, IAsyncDisposable
{
    public async Task ProduceAsync(string source, BaseMessage message, Dictionary<string, string>? attributes = null, CancellationToken cancellationToken = default)
    {
        var request = new SendMessageRequest
        {
            QueueUrl = source,
            MessageBody = JsonConvert.SerializeObject(message)
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

        if (source.EndsWith(Constants.FifoSuffix))
        {
            request.MessageGroupId = message.MessageGroupId; // TODO - should be the same for all messages in a group for it to be first in first out
            request.MessageDeduplicationId = Guid.NewGuid().ToString();
        }

        if (sqsClient != null)
        {
            await sqsClient.SendMessageAsync(request, cancellationToken);
        }
    }

    public async Task ProduceBatchAsync(string source, IEnumerable<BaseMessage> messages, Dictionary<string, string>? attributes = null, CancellationToken cancellationToken = default)
    {
        // batch messages by 10 items as SQS allows a maximum of 10 messages per batch
        foreach (var messageBatch in messages.GetByBatch(10))
        {
            var baseMessages = messageBatch.ToList();
            
            var request = new SendMessageBatchRequest
            {
                QueueUrl = source,
                Entries = baseMessages.Select(x => new SendMessageBatchRequestEntry(x.Id(), JsonConvert.SerializeObject(x)) { MessageGroupId = x.MessageGroupId }).ToList()
            };

            if (sqsClient != null)
            {
                await sqsClient.SendMessageBatchAsync(request, cancellationToken);
            }
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
        sqsClient?.Dispose();
        return new ValueTask(Task.CompletedTask);
    }

    public async ValueTask DisposeAsync()
    {
        sqsClient?.Dispose();
        await Task.CompletedTask;
    }
}
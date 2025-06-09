using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Topica.Aws.Contracts;
using Topica.Aws.Helpers;
using Topica.Contracts;
using Topica.Helpers;
using Topica.Infrastructure.Contracts;
using Topica.Messages;

namespace Topica.Aws.Producer;

public class AwsQueueProducer(string producerName, IPollyRetryService pollyRetryService, IAwsQueueService awsQueueService, IAmazonSQS? sqsClient, bool isFifo, ILogger logger) : IProducer
{
    public async Task ProduceAsync(string source, BaseMessage message, Dictionary<string, string>? attributes = null, CancellationToken cancellationToken = default)
    {
        var queueUrl = await GetQueueUrl(source, cancellationToken);
        
        var request = new SendMessageRequest
        {
            QueueUrl = queueUrl,
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

        if (queueUrl.EndsWith(Constants.FifoSuffix))
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
        var queueUrl = await GetQueueUrl(source, cancellationToken);
        
        // batch messages by 10 items as SQS allows a maximum of 10 messages per batch
        foreach (var messageBatch in messages.GetByBatch(10))
        {
            var baseMessages = messageBatch.ToList();
            
            var request = new SendMessageBatchRequest
            {
                QueueUrl = queueUrl,
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

    public async ValueTask DisposeAsync()
    {
        sqsClient?.Dispose();
        await Task.CompletedTask;
    }
    
    private async Task<string> GetQueueUrl(string queueName, CancellationToken cancellationToken)
    {
        var queueUrl = await pollyRetryService.WaitAndRetryAsync<Exception, string?>
        (
            5,
            _ => TimeSpan.FromSeconds(3),
            (delegateResult, ts, index, context) => logger.LogWarning("**** RETRY: {Name}: Retry attempt: {RetryAttempt} - Retry in {RetryDelayTotalSeconds} - Result: {Result}", nameof(AwsQueueProducer), index, ts, delegateResult.Exception?.Message ?? "The result did not pass the result condition."),
            result => string.IsNullOrWhiteSpace(result) || !result.StartsWith("http"),
            () => awsQueueService.GetQueueUrlAsync(queueName, isFifo, cancellationToken),
            false
        );

        if (queueUrl == null)
        {
            throw new Exception($"No queue url found for name: {queueName}");
        }
        
        return queueUrl;
    }
}
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
using Topica.Messages;
using Topica.Settings;

namespace Topica.Aws.Producer;

internal class AwsQueueProducer(string producerName, IPollyRetryService pollyRetryService, IAwsQueueService awsQueueService, IAmazonSQS? sqsClient, MessagingSettings messagingSettings, ILogger logger) : IProducer
{
    public string Source => TopicQueueHelper.AddTopicQueueNameFifoSuffix(messagingSettings.Source, messagingSettings.AwsIsFifoQueue);

    public async Task ProduceAsync(BaseMessage message, Dictionary<string, string>? attributes, CancellationToken cancellationToken)
    {
        var attributesToUse = attributes ?? new Dictionary<string, string>();
        attributesToUse.Add("ProducerName", producerName);
        
        var queueUrl = await GetQueueUrl(messagingSettings.Source, cancellationToken);
        
        var request = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = JsonConvert.SerializeObject(message),
            MessageAttributes = attributesToUse.ToDictionary(item => item.Key, item => new MessageAttributeValue
            {
                StringValue = item.Value,
                DataType = "String"
            })
        };

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

    public async Task ProduceBatchAsync(IEnumerable<BaseMessage> messages, Dictionary<string, string>? attributes, CancellationToken cancellationToken)
    {
        var attributesToUse = attributes ?? new Dictionary<string, string>();
        attributesToUse.Add("ProducerName", producerName);
        
        var queueUrl = await GetQueueUrl(messagingSettings.Source, cancellationToken);
        
        // batch messages by 10 items as SQS allows a maximum of 10 messages per batch
        foreach (var messageBatch in messages.GetByBatch(10))
        {
            var baseMessages = messageBatch.ToList();
            
            var request = new SendMessageBatchRequest
            {
                QueueUrl = queueUrl,
                Entries = baseMessages.Select(x => new SendMessageBatchRequestEntry(x.Id.ToString(), JsonConvert.SerializeObject(x))
                {
                    Id = Guid.NewGuid().ToString(),
                    MessageGroupId = x.MessageGroupId, 
                    MessageAttributes = attributesToUse.ToDictionary(item => item.Key, item => new MessageAttributeValue
                    {
                        StringValue = item.Value,
                        DataType = "String"
                    })
                }).ToList()
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
        // sqsClient?.Dispose();
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
            ct => awsQueueService.GetQueueUrlAsync(queueName, messagingSettings.AwsIsFifoQueue, ct),
            false,
            cancellationToken
        );

        if (queueUrl == null)
        {
            throw new Exception($"No queue url found for name: {queueName}");
        }
        
        return queueUrl;
    }
}
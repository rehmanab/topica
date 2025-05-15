using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Topica.Aws.Contracts;
using Topica.Aws.Messages;
using Topica.Aws.Queues;
using Topica.Messages;

namespace Topica.Aws.Services
{
    public class AwsQueueService(
        IAmazonSQS client,
        IAwsQueueCreationFactory awsQueueCreationFactory,
        IAwsSqsConfigurationBuilder awsSqsConfigurationBuilder,
        ILogger<AwsQueueService> logger) : IAwsQueueService
    {
        private const string AwsNonExistentQueue = "AWS.SimpleQueueService.NonExistentQueue";

        public async Task<bool> QueueExistsByNameAsync(string queueName)
        {
            var queueUrl = await GetQueueUrlAsync(queueName);

            return !string.IsNullOrWhiteSpace(queueUrl);
        }

        public async Task<bool> QueueExistsByUrlAsync(string queueUrl)
        {
            try
            {
                var attributes = await GetAttributesByQueueUrl(queueUrl, new []{ AwsQueueAttributes.QueueArnName });

                return attributes.ContainsKey(AwsQueueAttributes.QueueArnName) && !string.IsNullOrWhiteSpace(attributes[AwsQueueAttributes.QueueArnName]);
            }
            catch (AmazonSQSException ex)
            {
                if (ex.ErrorCode == AwsNonExistentQueue) return false;

                logger.LogError(ex, "SQS: Error - {ExMessage}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SQS: Error - {ExMessage}", ex.Message);
                throw;
            }
        }

        public async Task<string?> GetQueueUrlAsync(string queueName)
        {
            var queueUrl = (await GetQueueUrlsAsync(queueName, false))
                .ToList()
                .FirstOrDefault(x => x.EndsWith(queueName, StringComparison.InvariantCultureIgnoreCase));
            
            return queueUrl;
        }

        public async Task<IEnumerable<string>> GetQueueNamesByPrefix(string queueNamePrefix = "")
        {
            return await GetQueueUrlsAsync(queueNamePrefix, true);
        }

        public async Task<IEnumerable<string>> GetQueueUrlsByPrefix(string queueNamePrefix = "")
        {
            return await GetQueueUrlsAsync(queueNamePrefix, false);
        }

        public async Task<IDictionary<string, string>> GetAttributesByQueueName(string queueName, IEnumerable<string>? attributeNames = null)
        {
            var queueUrl = await GetQueueUrlAsync(queueName);

            if(string.IsNullOrWhiteSpace(queueUrl)) throw new ApplicationException($"SQS: GetAttributesByQueueName: {queueName} does not exist");

            return await GetAttributesByQueueUrl(queueUrl, attributeNames);
        }

        public async Task<IDictionary<string, string>> GetAttributesByQueueUrl(string queueUrl, IEnumerable<string>? attributeNames = null)
        {
            var getQueueAttributesRequest = new GetQueueAttributesRequest
            {
                AttributeNames = attributeNames?.ToList() ?? new List<string> { AwsQueueAttributes.AllName },
                QueueUrl = queueUrl
            };

            var queueAttributes = (await client.GetQueueAttributesAsync(getQueueAttributesRequest)).Attributes;
            queueAttributes.Add("QueueUrl", queueUrl);

            return queueAttributes;
        }

        public async Task<string> CreateQueueAsync(string queueName, AwsQueueCreationType awsQueueCreationType)
        {
            var configuration = awsSqsConfigurationBuilder.BuildWithCreationTypeQueue(awsQueueCreationType);

            return await CreateQueueAsync(queueName, configuration);
        }

        public async IAsyncEnumerable<string> CreateQueuesAsync(IEnumerable<string> queueNames, AwsSqsConfiguration awsSqsConfiguration)
        {
            foreach (var queueName in queueNames)
            {
                yield return await CreateQueueAsync(queueName, awsSqsConfiguration);
            }
        }

        public async Task<string> CreateQueueAsync(string queueName, AwsSqsConfiguration awsSqsConfiguration)
        {
            var createQueueType = awsSqsConfiguration.CreateErrorQueue.HasValue && awsSqsConfiguration.CreateErrorQueue.Value
                ? AwsQueueCreationType.WithErrorQueue
                : AwsQueueCreationType.SoleQueue;

            var queueCreator = awsQueueCreationFactory.Create(createQueueType);

            return await queueCreator.CreateQueue(queueName, awsSqsConfiguration);
        }

        //TODO - Update method to update all error queues redrive MaxReceiveCount property
        public async Task<bool> UpdateQueueAttributesAsync(string queueUrl, AwsSqsConfiguration configuration)
        {
            var response = await client.SetQueueAttributesAsync(queueUrl, configuration.QueueAttributes.GetAttributeDictionary());

            return response.HttpStatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> SendSingleAsync<T>(string queueUrl, T message)
        {
            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = JsonConvert.SerializeObject(message)
            };

            if (queueUrl.EndsWith(".fifo"))
            {
                sendMessageRequest.MessageGroupId = Guid.NewGuid().ToString();
                sendMessageRequest.MessageDeduplicationId = Guid.NewGuid().ToString();
            }

            var result = await client.SendMessageAsync(sendMessageRequest);

            return result.HttpStatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> SendMultipleAsync<T>(string queueUrl, IEnumerable<T> messages)
        {
            var isFifoQueue = queueUrl.EndsWith(".fifo");
            
            var requestEntries = 
                messages
                .Select((x, index) =>
                {
                    var entry = new SendMessageBatchRequestEntry(index.ToString(), JsonConvert.SerializeObject(x));
                    
                    if (isFifoQueue)
                    {
                        entry.MessageGroupId = Guid.NewGuid().ToString();
                        entry.MessageDeduplicationId = Guid.NewGuid().ToString();
                    }
                    
                    return entry;
                })
                .ToList();

            var request = new SendMessageBatchRequest
            {
                QueueUrl = queueUrl,
                Entries = requestEntries
            };

            logger.LogDebug($"SQS: sending multiple batched : {requestEntries.Count} messages");
            var response = await client.SendMessageBatchAsync(request);
            logger.LogError($"SQS: {response.Successful.Count} messages sent");

            if (!response.Failed.Any())
            {
                return true;
            }
            
            logger.LogError($"SQS: {response.Failed.Count} messages failed to send");
            foreach (var failed in response.Failed)
            {
                logger.LogError($"SQS: failed messageId: {failed.Id}, Code: {failed.Code}, Message: {failed.Message}, SenderFault: {failed.SenderFault}");
            }

            return false;
        }

        public async IAsyncEnumerable<T> StartReceive<T>(string queueUrl, [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : BaseMessage
        {
            var receiveMessageRequest = new ReceiveMessageRequest { QueueUrl = queueUrl };

            while (!cancellationToken.IsCancellationRequested)
            {
                var receiveMessageResponse = await client.ReceiveMessageAsync(receiveMessageRequest, cancellationToken);

                if (receiveMessageResponse?.Messages == null)
                {
                    await Task.Delay(1000, cancellationToken);
                    continue;
                }
                
                foreach (var message in receiveMessageResponse.Messages)
                {
                    if(message == null) continue;
                    
                    logger.LogDebug("SQS: Original Message from AWS: {SerializeObject}", JsonConvert.SerializeObject(message));

                    //Serialise normal SQS message body
                    var baseMessage = BaseMessage.Parse<BaseMessage>(message.Body);
                    var messageBody = message.Body;
                    
                    if (baseMessage == null)
                    {
                        logger.LogWarning("SQS: message body could not be serialized into Message ({MessageId}): {MessageBody}", message.MessageId, message.Body);
                        continue;
                    }
                    
                    // SNS notification sent, our message will be the Message property
                    if (baseMessage.Type == "Notification")
                    {
                        var notification = AwsNotification.Parse(messageBody);
                        if (notification == null)
                        {
                            logger.LogError("SQS: Error: could not convert Notification to AwsNotification object");
                            continue;
                        }
                        baseMessage = BaseMessage.Parse<BaseMessage>(notification.Message);
                        messageBody = notification.Message;
                    }

                    yield return (T)baseMessage;
                }
            }
        }

        public async Task<bool> DeleteMessageAsync(string queueUrl, string receiptHandle)
        {
            var response = await client.DeleteMessageAsync(queueUrl, receiptHandle);

            return response.HttpStatusCode == HttpStatusCode.OK;
        }

        private async Task<IEnumerable<string>> GetQueueUrlsAsync(string queueNamePrefix, bool nameOnly)
        {
            var response = await client.ListQueuesAsync(queueNamePrefix);
            var queueUrls = response.QueueUrls;
            
            if (queueUrls == null || !queueUrls.Any()) return [];

            var items = new List<string>();
            foreach (var queueUrl in queueUrls)
            {
                if (string.IsNullOrWhiteSpace(queueUrl) || !queueUrl.Contains("/")) continue;

                if (nameOnly)
                {
                    var lastEntryQueueName = queueUrl.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).LastOrDefault();

                    if (!string.IsNullOrEmpty(lastEntryQueueName))
                    {
                        items.Add(lastEntryQueueName);
                    }
                }
                else
                {
                    items.Add(queueUrl);
                }
            }

            return items;
        }
    }
}
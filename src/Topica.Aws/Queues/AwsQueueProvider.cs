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
using Topica.Aws.Configuration;
using Topica.Aws.Contracts;
using Topica.Aws.Messages;
using Message = Amazon.SimpleNotificationService.Util.Message;

namespace Topica.Aws.Queues
{
    public class AwsQueueProvider : IQueueProvider
    {
        private const string AwsNonExistentQueue = "AWS.SimpleQueueService.NonExistentQueue";
        private readonly IAmazonSQS _client;
        private readonly IQueueCreationFactory _queueCreationFactory;
        private readonly ILogger<AwsQueueProvider> _logger;
        private readonly ISqsConfigurationBuilder _sqsConfigurationBuilder;

        public AwsQueueProvider(IAmazonSQS client, IQueueCreationFactory queueCreationFactory, ISqsConfigurationBuilder sqsConfigurationBuilder, ILogger<AwsQueueProvider> logger)
        {
            _client = client;
            _queueCreationFactory = queueCreationFactory;
            _logger = logger;
            _sqsConfigurationBuilder = sqsConfigurationBuilder;
        }

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

                _logger.LogError(ex, $"SQS: Error - {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SQS: Error - {ex.Message}");
                throw;
            }
        }

        public async Task<string> GetQueueUrlAsync(string queueName)
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

            var queueAttributes = (await _client.GetQueueAttributesAsync(getQueueAttributesRequest)).Attributes;
            queueAttributes.Add("QueueUrl", queueUrl);

            return queueAttributes;
        }

        public async Task<string> CreateQueueAsync(string queueName, QueueCreationType queueCreationType)
        {
            var configuration = _sqsConfigurationBuilder.BuildWithCreationTypeQueue(queueCreationType);

            return await CreateQueueAsync(queueName, configuration);
        }

        public async IAsyncEnumerable<string> CreateQueuesAsync(IEnumerable<string> queueNames, QueueConfiguration? queueConfiguration)
        {
            foreach (var queueName in queueNames)
            {
                yield return await CreateQueueAsync(queueName, queueConfiguration);
            }
        }

        public async Task<string> CreateQueueAsync(string queueName, QueueConfiguration? queueConfiguration)
        {
            var createQueueType = queueConfiguration.CreateErrorQueue.HasValue && queueConfiguration.CreateErrorQueue.Value
                ? QueueCreationType.WithErrorQueue
                : QueueCreationType.SoleQueue;

            var queueCreator = _queueCreationFactory.Create(createQueueType);

            return await queueCreator.CreateQueue(queueName, queueConfiguration);
        }

        //TODO - Update method to update all error queues redrive MaxReceiveCount property
        public async Task<bool> UpdateQueueAttributesAsync(string queueUrl, QueueConfiguration configuration)
        {
            var response = await _client.SetQueueAttributesAsync(queueUrl, configuration.QueueAttributes.GetAttributeDictionary());

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

            var result = await _client.SendMessageAsync(sendMessageRequest);

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

            _logger.LogDebug($"SQS: sending multiple batched : {requestEntries.Count} messages");
            var response = await _client.SendMessageBatchAsync(request);
            _logger.LogError($"SQS: {response.Successful.Count} messages sent");

            if (!response.Failed.Any())
            {
                return true;
            }
            
            _logger.LogError($"SQS: {response.Failed.Count} messages failed to send");
            foreach (var failed in response.Failed)
            {
                _logger.LogError($"SQS: failed messageId: {failed.Id}, Code: {failed.Code}, Message: {failed.Message}, SenderFault: {failed.SenderFault}");
            }

            return false;
        }

        public async IAsyncEnumerable<T> StartReceive<T>(string queueUrl, [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : BaseAwsMessage
        {
            var receiveMessageRequest = new ReceiveMessageRequest { QueueUrl = queueUrl };

            while (true)
            {
                var receiveMessageResponse = await _client.ReceiveMessageAsync(receiveMessageRequest, cancellationToken);

                foreach (var message in receiveMessageResponse.Messages)
                {
                    _logger.LogDebug($"SQS: Original Message from AWS: {JsonConvert.SerializeObject(message)}");

                    //Serialise normal SQS message body
                    BaseAwsMessage baseAwsMessage = JsonConvert.DeserializeObject<T>(message.Body)!;
                    baseAwsMessage.Type = "Queue";

                    if (baseAwsMessage.ConversationId == Guid.Empty)
                    {
                        //Otherwise serialise to an SnsMessage
                        var snsMessage = Message.ParseMessage(message.Body);
                        baseAwsMessage = JsonConvert.DeserializeObject<T>(snsMessage.MessageText)!;
                        if (baseAwsMessage.ConversationId == Guid.Empty)
                        {
                            _logger.LogError("SQS: Error: could not convert message to base message");
                            continue;
                        }

                        baseAwsMessage.Type = snsMessage.Type;
                        baseAwsMessage.TopicArn = snsMessage.TopicArn;
                    }
                    
                    _logger.LogDebug($"SQS: baseMessage: {JsonConvert.SerializeObject(baseAwsMessage)}");

                    baseAwsMessage.MessageId = message.MessageId;
                    baseAwsMessage.ReceiptHandle = message.ReceiptHandle;

                    yield return (T)baseAwsMessage;
                }

                if (cancellationToken.IsCancellationRequested) break;
            }
        }

        public async Task<bool> DeleteMessageAsync(string queueUrl, string receiptHandle)
        {
            var response = await _client.DeleteMessageAsync(queueUrl, receiptHandle);

            return response.HttpStatusCode == HttpStatusCode.OK;
        }

        private async Task<IEnumerable<string>> GetQueueUrlsAsync(string queueNamePrefix, bool nameOnly)
        {
            var response = await _client.ListQueuesAsync(queueNamePrefix);
            var queueUrls = response.QueueUrls;

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
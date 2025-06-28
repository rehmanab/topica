using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Topica.Aws.Contracts;
using Topica.Aws.Helpers;
using Topica.Aws.Queues;
using Topica.Aws.Strategy;

namespace Topica.Aws.Services
{
    public class AwsQueueService(IAmazonSQS client, ILogger<AwsQueueService> logger) : IAwsQueueService
    {
        public async Task<string?> GetQueueUrlAsync(string queueName, CancellationToken cancellationToken)
        {
            return await GetQueueUrlAsync(queueName, false, cancellationToken);
        }

        public async Task<string?> GetQueueUrlAsync(string queueName, bool isFifo, CancellationToken cancellationToken)
        {
            queueName = TopicQueueHelper.AddTopicQueueNameFifoSuffix(queueName, isFifo);
            
            try
            {
                var response = await client.GetQueueUrlAsync(queueName, cancellationToken);

                if (response != null && !string.IsNullOrWhiteSpace(response.QueueUrl))
                {
                    return response.QueueUrl;
                }

                return null;
            }
            catch (QueueDoesNotExistException)
            {
                return null;
            }
        }

        public async Task<IDictionary<string, string>> GetAttributesByQueueUrl(string queueUrl, IEnumerable<string>? attributeNames = null)
        {
            var getQueueAttributesRequest = new GetQueueAttributesRequest
            {
                AttributeNames = attributeNames?.ToList() ?? [AwsQueueAttributes.AllName],
                QueueUrl = queueUrl
            };

            var queueAttributes = (await client.GetQueueAttributesAsync(getQueueAttributesRequest)).Attributes;
            queueAttributes.Add("QueueUrl", queueUrl);

            return queueAttributes;
        }

        public async Task<string> CreateQueueAsync(string queueName, AwsSqsConfiguration? awsSqsConfiguration, CancellationToken cancellationToken)
        {
            var queueUrl = await GetQueueUrlAsync(queueName, awsSqsConfiguration?.QueueAttributes.IsFifoQueue ?? false, cancellationToken);
            
            if (!string.IsNullOrWhiteSpace(queueUrl))
            {
                logger.LogDebug("**** EXISTS: queue already exists, queueUrl: {QueueUrl}", queueUrl);
                return queueUrl;
            }
            
            var createQueueType = awsSqsConfiguration.CreateErrorQueue
                ? AwsQueueCreationType.WithErrorQueue
                : AwsQueueCreationType.SoleQueue;

            IAwsQueueCreator queueCreator = createQueueType switch
            {
                AwsQueueCreationType.SoleQueue => new AwsSoleQueueCreator(client),
                AwsQueueCreationType.WithErrorQueue => new AwsQueueWithErrorsCreator(client),
                _ => throw new ApplicationException($"Can not find queue creator for: {createQueueType}")
            };

            queueUrl = await queueCreator.CreateQueue(queueName, awsSqsConfiguration, cancellationToken);
            logger.LogDebug("**** CREATED QUEUE SUCCESS: QueueUrl: {QueueUrl}", queueUrl);
            
            return queueUrl;
        }

        //TODO - Update method to update all error queues redrive MaxReceiveCount property
        public async Task<bool> UpdateQueueAttributesAsync(string queueUrl, AwsSqsConfiguration configuration)
        {
            var response = await client.SetQueueAttributesAsync(queueUrl, configuration.QueueAttributes.GetAttributeDictionary());

            return response.HttpStatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> DeleteQueueAsync(string? queueUrl, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(queueUrl)) return true;
            
            try       
            {
                var response = await client.DeleteQueueAsync(queueUrl, cancellationToken);
                return response.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (QueueDoesNotExistException)
            {
                logger.LogWarning("Queue does not exist: {QueueUrl}", queueUrl);
                return false;
            }
        }

        public async Task<bool> DeleteMessageAsync(string queueUrl, string receiptHandle)
        {
            var response = await client.DeleteMessageAsync(queueUrl, receiptHandle);

            return response.HttpStatusCode == HttpStatusCode.OK;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Topica.Aws.Builders;
using Topica.Aws.Contracts;
using Topica.Aws.Queues;
using Topica.Messages;

namespace Topica.Aws.Services
{
    public class AwsTopicService(IAmazonSimpleNotificationService snsClient, IAwsQueueService awsQueueService, IAwsPolicyBuilder awsPolicyBuilder, ISqsConfigurationBuilder sqsConfigurationBuilder, ILogger<AwsTopicService> logger) : IAwsTopicService
    {
        private const string FifoSuffix = ".fifo";
        
        public async Task<string?> GetTopicArnAsync(string topicName, bool isFifo)
        {
            string? topicArnFound = null;
            var found = false;
            var nextToken = string.Empty;

            do
            {
                var response = await snsClient.ListTopicsAsync(nextToken);
                if (response?.Topics == null) break;

                topicArnFound = response.Topics.Select(x => x.TopicArn).SingleOrDefault(y => y.ToLower().EndsWith($"{topicName.ToLower()}{(isFifo && !topicName.ToLower().EndsWith(FifoSuffix) ? FifoSuffix : "")}"));
                if (!string.IsNullOrWhiteSpace(topicArnFound)) found = true;
                nextToken = response.NextToken;
            }
            while (!found && !string.IsNullOrEmpty(nextToken));

            return !string.IsNullOrWhiteSpace(topicArnFound) ? topicArnFound : null;
        }

        public async Task<bool> TopicExistsAsync(string topicName)
        {
            return !string.IsNullOrWhiteSpace(await GetTopicArnAsync(topicName, false));
        }

        public async Task AuthorizeS3ToPublishByTopicNameAsync(string topicName, string bucketName)
        {
            var topicArn = await GetTopicArnAsync(topicName, false);

            await AuthorizeS3ToPublishByTopicArnAsync(topicArn, bucketName);
        }

        public async Task AuthorizeS3ToPublishByTopicArnAsync(string? topicArn, string bucketName)
        {
            await snsClient.AuthorizeS3ToPublishAsync(topicArn, bucketName);
        }

        public async Task<string?> CreateTopicArnAsync(string topicName, bool isFifoQueue)
        {
            var topicArn = await GetTopicArnAsync(topicName, isFifoQueue);

            if (!string.IsNullOrWhiteSpace(topicArn))
            {
                logger.LogInformation("SNS: CreateTopicArnAsync topic {Name} already exists!", topicName);
                return topicArn;
            }

            var createTopicRequest = new CreateTopicRequest
            {
                Name = $"{topicName}{(isFifoQueue && !topicName.EndsWith(FifoSuffix) ? FifoSuffix : "")}",
                Attributes = new Dictionary<string, string>{{"FifoTopic", isFifoQueue ? "true" : "false"}}
            };
            var response = await snsClient.CreateTopicAsync(createTopicRequest);

            return response.HttpStatusCode == HttpStatusCode.OK ? response.TopicArn : null;
        }

        public async Task SendToTopicAsync(string? topicArn, BaseMessage message)
        {
            var request = new PublishRequest
            {
                TopicArn = topicArn,
                Message = JsonConvert.SerializeObject(message)
            };

            //TODO - use polly for retry incase topic has just been created
            var publishResponse = await snsClient.PublishAsync(request);

            logger.LogDebug($"SNS: SendToTopicAsync response: {publishResponse.HttpStatusCode}");
        }

        public async Task SendToTopicByTopicNameAsync(string topicName, BaseMessage message)
        {
            var topicArn = await GetTopicArnAsync(topicName, false);

            await SendToTopicAsync(topicArn, message);
        }

        public async Task<bool> SubscriptionExistsAsync(string? topicArn, string endpointArn)
        {
            var topicSubscriptionArns = await ListTopicSubscriptionsAsync(topicArn);

            return topicSubscriptionArns.Any(x => string.Equals(endpointArn, x));
        }

        public async Task<IEnumerable<string>> ListTopicSubscriptionsAsync(string? topicArn)
        {
            var response = await snsClient.ListSubscriptionsByTopicAsync(topicArn, string.Empty);
            
            if (response?.Subscriptions == null)
            {
                return new List<string>();
            }

            return response.HttpStatusCode == HttpStatusCode.OK ? response.Subscriptions.Select(x => x.Endpoint) : new List<string>();
        }

        public async Task<string?> CreateTopicWithOptionalQueuesSubscribedAsync(string topicName, string[] queueNames)
        {
            return await CreateTopicWithOptionalQueuesSubscribedAsync
            (
                topicName, queueNames, sqsConfigurationBuilder.BuildWithCreationTypeQueue(QueueCreationType.SoleQueue)
            );
        }

        public async Task<string?> CreateTopicWithOptionalQueuesSubscribedAsync(string topicName, string[] queueNames, SqsConfiguration? sqsConfiguration)
        {
            sqsConfiguration ??= sqsConfigurationBuilder.BuildQueue();
            
            var topicArn = await CreateTopicArnAsync(topicName, sqsConfiguration.QueueAttributes.IsFifoQueue);

            foreach (var queue in queueNames)
            {
                var queueName = $"{queue}{(sqsConfiguration.QueueAttributes.IsFifoQueue ? FifoSuffix : "")}";
                logger.LogDebug("SNS: getting queueUrl for: {QueueName}", queueName);
                var queueUrl = await awsQueueService.GetQueueUrlAsync(queueName);

                if (string.IsNullOrWhiteSpace(queueUrl))
                {
                    logger.LogDebug("SNS: queue does not exist, creating queue");
                    queueUrl = await awsQueueService.CreateQueueAsync(queueName, sqsConfiguration);
                    logger.LogDebug("SNS: queue created, queueUrl: {QueueUrl}", queueUrl);
                }

                var properties = await awsQueueService.GetAttributesByQueueUrl(queueUrl, new List<string> { AwsQueueAttributes.QueueArnName });

                var queueArn = properties[AwsQueueAttributes.QueueArnName];
                logger.LogDebug("SNS: got queue Arn: {QueueArn}", queueArn);

                if (!await SubscriptionExistsAsync(topicArn, queueArn))
                {
                    await snsClient.SubscribeAsync(topicArn, "sqs", queueArn);
                    logger.LogDebug("SNS: NEW subscription to topic create");
                }
                else
                {
                    logger.LogDebug("SNS: queue ALREADY subscribed to topic");
                }

                //Set access policy
                var accessPolicy = awsPolicyBuilder.BuildQueueAllowPolicyForTopicToSendMessage(queueUrl, queueArn, topicArn!);
                if(await awsQueueService.UpdateQueueAttributesAsync(queueUrl, sqsConfigurationBuilder.BuildUpdatePolicyQueue(accessPolicy)))
                    logger.LogDebug("SNS: Updated queue policy to allow messages from topic");
                else
                    throw new ApplicationException($"Could not update the policy for queue: {queueName} to receive messages from topic: {topicName}");
            }

            logger.LogDebug("SNS: Done creating topic: {TopicArn} and subscribing queues", topicArn);

            return topicArn;
        }

        public async Task<bool> DeleteTopicArnAsync(string topicName)
        {
            var response = await snsClient.DeleteTopicAsync(topicName);

            var success = response.HttpStatusCode == HttpStatusCode.OK;

            logger.LogDebug(success
                ? $"SNS: deleted topic: {topicName}"
                : $"SNS: error deleting topic: {topicName}, response: {JsonConvert.SerializeObject(response)}");

            return success;
        }
    }
}
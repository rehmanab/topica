using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Topica.Aws.Configuration;
using Topica.Aws.Contracts;
using Topica.Aws.Queues;
using Topica.Messages;

namespace Topica.Aws.Topics
{
    public class AwsTopicProvider : ITopicProvider
    {
        private readonly IAwsPolicyBuilder _awsPolicyBuilder;
        private readonly ISqsConfigurationBuilder _sqsConfigurationBuilder;
        private readonly ILogger<AwsTopicProvider> _logger;
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly IQueueProvider _queueProvider;

        public AwsTopicProvider(IAmazonSimpleNotificationService snsClient, IQueueProvider queueProvider, 
            IAwsPolicyBuilder awsPolicyBuilder, ISqsConfigurationBuilder sqsConfigurationBuilder, ILogger<AwsTopicProvider> logger)
        {
            _snsClient = snsClient;
            _queueProvider = queueProvider;
            _awsPolicyBuilder = awsPolicyBuilder;
            _sqsConfigurationBuilder = sqsConfigurationBuilder;
            _logger = logger;
        }

        public async Task<string?> GetTopicArnAsync(string topicName)
        {
            var topic = await _snsClient.FindTopicAsync(topicName);

            return !string.IsNullOrWhiteSpace(topic?.TopicArn) ? topic.TopicArn : null;
        }

        public async Task<bool> TopicExistsAsync(string topicName)
        {
            return !string.IsNullOrWhiteSpace(await GetTopicArnAsync(topicName));
        }

        public async Task AuthorizeS3ToPublishByTopicNameAsync(string topicName, string bucketName)
        {
            var topicArn = await GetTopicArnAsync(topicName);

            await AuthorizeS3ToPublishByTopicArnAsync(topicArn, bucketName);
        }

        public async Task AuthorizeS3ToPublishByTopicArnAsync(string? topicArn, string bucketName)
        {
            await _snsClient.AuthorizeS3ToPublishAsync(topicArn, bucketName);
        }

        public async Task<string?> CreateTopicArnAsync(string topicName, bool? isFifoQueue)
        {
            var topicArn = await GetTopicArnAsync(topicName);

            if (!string.IsNullOrWhiteSpace(topicArn)) return topicArn;

            var response = await _snsClient.CreateTopicAsync(new CreateTopicRequest
            {
                Name = $"{topicName}{(isFifoQueue.HasValue && isFifoQueue.Value ? ".fifo" : "")}",
                Attributes = new Dictionary<string, string>{{"FifoTopic", isFifoQueue.HasValue && isFifoQueue.Value ? "true" : "false"}}
            });

            return response.HttpStatusCode == HttpStatusCode.OK ? response.TopicArn : null;
        }

        public async Task SendToTopicAsync(string? topicArn, Message message)
        {
            var request = new PublishRequest
            {
                TopicArn = topicArn,
                Message = JsonConvert.SerializeObject(message)
            };

            //TODO - use polly for retry incase topic has just been created
            var publishResponse = await _snsClient.PublishAsync(request);

            _logger.LogDebug($"SNS: SendToTopicAsync response: {publishResponse.HttpStatusCode}");
        }

        public async Task SendToTopicByTopicNameAsync(string topicName, Message message)
        {
            var topicArn = await GetTopicArnAsync(topicName);

            await SendToTopicAsync(topicArn, message);
        }

        public async Task<bool> SubscriptionExistsAsync(string? topicArn, string endpointArn)
        {
            var topicSubscriptionArns = await ListTopicSubscriptionsAsync(topicArn);

            return topicSubscriptionArns.Any(x => string.Equals(endpointArn, x));
        }

        public async Task<IEnumerable<string>> ListTopicSubscriptionsAsync(string? topicArn)
        {
            var response = await _snsClient.ListSubscriptionsByTopicAsync(topicArn, string.Empty);

            return response.HttpStatusCode == HttpStatusCode.OK ? response.Subscriptions.Select(x => x.Endpoint) : new List<string>();
        }

        public async Task<string?> CreateTopicWithOptionalQueuesSubscribedAsync(string topicName, string[] queueNames)
        {
            return await CreateTopicWithOptionalQueuesSubscribedAsync
            (
                topicName, queueNames, _sqsConfigurationBuilder.BuildWithCreationTypeQueue(QueueCreationType.SoleQueue)
            );
        }

        public async Task<string?> CreateTopicWithOptionalQueuesSubscribedAsync(string topicName, string[] queueNames, QueueConfiguration? queueConfiguration)
        {
            queueConfiguration ??= _sqsConfigurationBuilder.BuildDefaultQueue();
            
            var topicArn = await GetTopicArnAsync(topicName);

            if (string.IsNullOrWhiteSpace(topicArn))
            {
                _logger.LogDebug($"SNS: Topic does not exist, creating topic: {topicName}");
                topicArn = await CreateTopicArnAsync(topicName, queueConfiguration.QueueAttributes.IsFifoQueue);
            }

            foreach (var queueName in queueNames)
            {
                _logger.LogDebug($"SNS: getting queueUrl for: {queueName}");
                var queueUrl = await _queueProvider.GetQueueUrlAsync(queueName);

                if (string.IsNullOrWhiteSpace(queueUrl))
                {
                    _logger.LogDebug("SNS: queue does not exist, creating queue");
                    queueUrl = await _queueProvider.CreateQueueAsync(queueName, queueConfiguration);
                    _logger.LogDebug($"SNS: queue created, queueUrl: {queueUrl}");
                }

                var properties = await _queueProvider.GetAttributesByQueueUrl(queueUrl, new List<string> { AwsQueueAttributes.QueueArnName });

                var queueArn = properties[AwsQueueAttributes.QueueArnName];
                _logger.LogDebug($"SNS: got queue Arn: {queueArn}");

                if (!await SubscriptionExistsAsync(topicArn, queueArn))
                {
                    await _snsClient.SubscribeAsync(topicArn, "sqs", queueArn);
                    _logger.LogDebug("SNS: NEW subscription to topic create");
                }
                else
                {
                    _logger.LogDebug("SNS: queue ALREADY subscribed to topic");
                }

                //TODO - Get properties from above and create method to parse policy, check if already has allow all users to send message from topic?
                //Set access policy
                var accessPolicy = _awsPolicyBuilder.BuildQueueAllowPolicyForTopicToSendMessage(queueUrl, queueArn, topicArn!);
                if(await _queueProvider.UpdateQueueAttributesAsync(queueUrl, _sqsConfigurationBuilder.BuildUpdatePolicyQueue(accessPolicy)))
                    _logger.LogDebug("SNS: Updated queue policy to allow messages from topic");
                else
                    throw new ApplicationException($"Could not update the policy for queue: {queueName} to receive messages from topic: {topicName}");
            }

            _logger.LogDebug("SNS: Done !");

            return topicArn;
        }

        public async Task<bool> DeleteTopicArnAsync(string topicName)
        {
            var response = await _snsClient.DeleteTopicAsync(topicName);

            var success = response.HttpStatusCode == HttpStatusCode.OK;

            _logger.LogDebug(success
                ? $"SNS: deleted topic: {topicName}"
                : $"SNS: error deleting topic: {topicName}, response: {JsonConvert.SerializeObject(response)}");

            return success;
        }
    }
}
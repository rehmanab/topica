using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Auth.AccessControlPolicy;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Topica.Aws.Contracts;
using Topica.Aws.Helpers;
using Topica.Aws.Queues;
using Topica.Messages;

namespace Topica.Aws.Services
{
    public class AwsTopicService(IAmazonSimpleNotificationService snsClient, IAwsQueueService awsQueueService, ILogger<AwsTopicService> logger) : IAwsTopicService
    {
        public async IAsyncEnumerable<IEnumerable<Topic>> GetAllTopics(string? topicNamePrefix = null, bool? isFifo = false)
        {
            Func<Topic, bool> filterTopics = x =>
            {
                if (string.IsNullOrWhiteSpace(topicNamePrefix)) return true;

                if (string.IsNullOrWhiteSpace(x.TopicArn) || !x.TopicArn.Contains(':')) return false;

                var lastEntryTopicName = x.TopicArn.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).LastOrDefault();

                return !string.IsNullOrEmpty(lastEntryTopicName)
                       && lastEntryTopicName.StartsWith(topicNamePrefix, StringComparison.CurrentCultureIgnoreCase)
                       && (isFifo.HasValue && isFifo.Value ? lastEntryTopicName.EndsWith(Constants.FifoSuffix) : !lastEntryTopicName.EndsWith(Constants.FifoSuffix));
            };

            var response = await snsClient.ListTopicsAsync();

            if (response?.Topics == null || response.Topics.Count == 0) yield break;

            yield return response.Topics.Where(filterTopics);

            while (!string.IsNullOrWhiteSpace(response.NextToken))
            {
                response = await snsClient.ListTopicsAsync(response.NextToken);
                if (response?.Topics == null || response.Topics.Count == 0) yield break;
                yield return response.Topics.Where(filterTopics);
            }
        }

        public async Task<string?> GetTopicArnAsync(string topicName, bool isFifo)
        {
            string? topicArnFound = null;
            var found = false;
            var nextToken = string.Empty;

            do
            {
                var response = await snsClient.ListTopicsAsync(nextToken);
                if (response?.Topics == null) break;

                topicArnFound = response.Topics.Select(x => x.TopicArn).SingleOrDefault(y => y.ToLower().EndsWith($"{topicName.ToLower()}{(isFifo && !topicName.ToLower().EndsWith(Constants.FifoSuffix) ? Constants.FifoSuffix : "")}"));
                if (!string.IsNullOrWhiteSpace(topicArnFound)) found = true;
                nextToken = response.NextToken;
            } while (!found && !string.IsNullOrEmpty(nextToken));

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
                Name = $"{topicName}{(isFifoQueue && !topicName.EndsWith(Constants.FifoSuffix) ? Constants.FifoSuffix : "")}",
                Attributes = new Dictionary<string, string> { { "FifoTopic", isFifoQueue ? "true" : "false" } }
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

            //TODO - use polly for retry in case topic has just been created
            var publishResponse = await snsClient.PublishAsync(request);

            logger.LogDebug("SNS: SendToTopicAsync response: {PublishResponseHttpStatusCode}", publishResponse.HttpStatusCode);
        }

        public async Task<bool> SubscriptionExistsAsync(string? topicArn, string endpointArn)
        {
            var response = await snsClient.ListSubscriptionsByTopicAsync(topicArn, string.Empty);

            var topicSubscriptionArns = response?.Subscriptions == null ? new List<string>() : response.HttpStatusCode == HttpStatusCode.OK ? response.Subscriptions.Select(x => x.Endpoint) : new List<string>();

            return topicSubscriptionArns.Any(x => string.Equals(endpointArn, x));
        }

        public async Task<string?> CreateTopicWithOptionalQueuesSubscribedAsync(string topicName, string[] queueNames, AwsSqsConfiguration sqsConfiguration)
        {
            var topicArn = await CreateTopicArnAsync(topicName, sqsConfiguration.QueueAttributes.IsFifoQueue);

            foreach (var queue in queueNames)
            {
                var queueName = $"{queue}{(sqsConfiguration.QueueAttributes.IsFifoQueue ? Constants.FifoSuffix : "")}";
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
                var accessPolicy = BuildQueueAllowPolicyForTopicToSendMessage(queueArn, topicArn!);
                var awsSqsConfiguration = new AwsSqsConfiguration { QueueAttributes = new AwsQueueAttributes { Policy = accessPolicy } };

                if (await awsQueueService.UpdateQueueAttributesAsync(queueUrl, awsSqsConfiguration))
                {
                    logger.LogDebug("SNS: Updated queue policy to allow messages from topic");
                }
                else
                {
                    throw new ApplicationException($"Could not update the policy for queue: {queueName} to receive messages from topic: {topicName}");
                }
            }

            logger.LogDebug("SNS: Done creating topic: {TopicArn} and subscribing queues", topicArn);

            return topicArn;
        }

        private static string BuildQueueAllowPolicyForTopicToSendMessage(string queueArn, string topicArn)
        {
            const Statement.StatementEffect statementEffect = Statement.StatementEffect.Allow;
            var sourceArnWildcard = CreateResourceArnWildcard(topicArn);
            var principals = new[] { Principal.AllUsers };
            var actionIdentifiers = new[] { new ActionIdentifier("sqs:SendMessage") };

            return new Policy()
                .WithStatements(new Statement(statementEffect)
                    .WithResources(new Resource(queueArn))
                    .WithConditions(ConditionFactory.NewSourceArnCondition(sourceArnWildcard))
                    .WithPrincipals(principals)
                    .WithActionIdentifiers(actionIdentifiers)).ToJson();
        }

        private static string CreateResourceArnWildcard(string resourceArn)
        {
            if (string.IsNullOrWhiteSpace(resourceArn) ||
                !resourceArn.StartsWith("arn", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ApplicationException($"AwsPolicy: Seems not to be a valid ARN: {resourceArn}");
            }

            var index = resourceArn.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);

            if (index > 0)
                resourceArn = resourceArn.Substring(0, index + 1);
            else
                throw new ApplicationException($"AwsPolicy: Seems not to be a valid ARN: {resourceArn}");

            return resourceArn + "*";
        }
    }
}
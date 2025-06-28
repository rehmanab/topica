using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using Topica.Aws.Contracts;
using Topica.Aws.Queues;
using static Topica.Aws.Helpers.TopicQueueHelper;

namespace Topica.Aws.Strategy
{
    public class AwsQueueWithErrorsCreator(IAmazonSQS client) : IAwsQueueCreator
    {
        public async Task<string> CreateQueue(string queueName, AwsSqsConfiguration configuration, CancellationToken cancellationToken)
        {
            //Create Error Queue
            //Create normal queue passing in redrive policy
            queueName = RemoveTopicQueueNameFifoSuffix(queueName);
            
            var errorQueueUrl = await InternalCreateErrorQueue(AddTopicQueueNameErrorAndFifoSuffix(queueName, configuration.QueueAttributes.IsFifoQueue), configuration, cancellationToken);
            var errorQueueArn = await GetQueueArn(errorQueueUrl, cancellationToken);

            var redrivePolicy = $"{{\"maxReceiveCount\":\"{configuration.ErrorQueueMaxReceiveCount}\", \"deadLetterTargetArn\":\"{errorQueueArn}\"}}";
            
            var mainQueueUrl = await InternalMainCreateQueue(AddTopicQueueNameFifoSuffix(queueName, configuration.QueueAttributes.IsFifoQueue), configuration, redrivePolicy, cancellationToken);

            return mainQueueUrl;
        }

        private async Task<string> InternalCreateErrorQueue(string queueName, AwsSqsConfiguration configuration, CancellationToken cancellationToken)
        {
            return await InternalCreateQueue(queueName, configuration.QueueAttributes.GetAttributeDictionary(), cancellationToken);
        }

        private async Task<string> InternalMainCreateQueue(string queueName, AwsSqsConfiguration configuration, string? redrivePolicy, CancellationToken cancellationToken)
        {
            var attributeDictionary = configuration.QueueAttributes.GetAttributeDictionary();
            attributeDictionary.Add(AwsQueueAttributes.RedrivePolicyName, redrivePolicy);

            return await InternalCreateQueue(queueName, attributeDictionary, cancellationToken);
        }

        private async Task<string> InternalCreateQueue(string queueName, Dictionary<string, string?> attributes, CancellationToken cancellationToken)
        {
            var request = new CreateQueueRequest
            {
                QueueName = queueName,
                Attributes = attributes
            };

            var response = await client.CreateQueueAsync(request, cancellationToken);

            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                return response.QueueUrl;
            }

            throw new ApplicationException($"Error creating queue, response from AWS: { JsonConvert.SerializeObject(response) }");
        }

        private async Task<string> GetQueueArn(string queueUrl, CancellationToken cancellationToken)
        {
            var getQueueAttributesRequest = new GetQueueAttributesRequest
            {
                AttributeNames = [AwsQueueAttributes.QueueArnName],
                QueueUrl = queueUrl
            };

            var getQueueAttributesResponse = await client.GetQueueAttributesAsync(getQueueAttributesRequest, cancellationToken);

            return getQueueAttributesResponse.Attributes[AwsQueueAttributes.QueueArnName];
        }
    }
}
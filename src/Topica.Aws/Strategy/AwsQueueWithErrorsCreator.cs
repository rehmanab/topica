using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using Topica.Aws.Contracts;
using Topica.Aws.Policies;
using Topica.Aws.Queues;

namespace Topica.Aws.Strategy
{
    public class AwsQueueWithErrorsCreator : IAwsQueueCreator
    {
        private const string ErrorQueueSuffix = "_error";
        private const string FifoQueueSuffix = ".fifo";
        private const int DefaultMaxReceiveCount = 3;
        private readonly IAmazonSQS _client;

        public AwsQueueWithErrorsCreator(IAmazonSQS client)
        {
            _client = client;
        }

        public async Task<string> CreateQueue(string queueName, AwsSqsConfiguration? configuration)
        {
            //Create Error Queue
            //Create normal queue passing in redrive policy
            var isFifo = configuration.QueueAttributes.IsFifoQueue;
            if (isFifo) queueName = queueName.Replace(".fifo", string.Empty);
            var errorQueueUrl = await InternalCreateErrorQueue($"{queueName}{ErrorQueueSuffix}{(isFifo ? FifoQueueSuffix : "")}", configuration);
            var errorQueueArn = await GetQueueArn(errorQueueUrl);

            var redrivePolicy = new AwsRedrivePolicy(configuration.MaxReceiveCount ?? DefaultMaxReceiveCount, errorQueueArn).ToJson();
            
            var mainQueueUrl = await InternalMainCreateQueue($"{queueName}{(isFifo ? FifoQueueSuffix : "")}", configuration, redrivePolicy);

            return mainQueueUrl;
        }

        private async Task<string> InternalCreateErrorQueue(string queueName, AwsSqsConfiguration? configuration)
        {
            return await InternalCreateQueue(queueName, configuration.QueueAttributes.GetAttributeDictionary());
        }

        private async Task<string> InternalMainCreateQueue(string queueName, AwsSqsConfiguration? configuration, string? redrivePolicy)
        {
            var attributeDictionary = configuration.QueueAttributes.GetAttributeDictionary();
            attributeDictionary.Add(AwsQueueAttributes.RedrivePolicyName, redrivePolicy);

            return await InternalCreateQueue(queueName, attributeDictionary);
        }

        private async Task<string> InternalCreateQueue(string queueName, Dictionary<string, string?> attributes)
        {
            var request = new CreateQueueRequest
            {
                QueueName = queueName,
                Attributes = attributes
            };

            var response = await _client.CreateQueueAsync(request);

            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                return response.QueueUrl;
            }

            throw new ApplicationException($"Error creating queue, response from AWS: { JsonConvert.SerializeObject(response) }");
        }

        private async Task<string> GetQueueArn(string queueUrl)
        {
            var getQueueAttributesRequest = new GetQueueAttributesRequest
            {
                AttributeNames = new List<string> { AwsQueueAttributes.QueueArnName },
                QueueUrl = queueUrl
            };

            var queueAttributes = (await _client.GetQueueAttributesAsync(getQueueAttributesRequest)).Attributes;

            return queueAttributes[AwsQueueAttributes.QueueArnName];
        }
    }
}
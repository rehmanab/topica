using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Topica.Aws.Contracts;
using Topica.Aws.Queues;
using Topica.Aws.Strategy;

namespace Topica.Aws.Services
{
    public class AwsQueueService(IAmazonSQS client) : IAwsQueueService
    {
        public async Task<string?> GetQueueUrlAsync(string queueName)
        {
            var queueUrl = (await GetQueueUrlsAsync(queueName, false))
                .ToList()
                .FirstOrDefault(x => x.EndsWith(queueName, StringComparison.InvariantCultureIgnoreCase));

            return queueUrl;
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

        public async Task<string> CreateQueueAsync(string queueName, AwsSqsConfiguration awsSqsConfiguration)
        {
            var createQueueType = awsSqsConfiguration.CreateErrorQueue.HasValue && awsSqsConfiguration.CreateErrorQueue.Value
                ? AwsQueueCreationType.WithErrorQueue
                : AwsQueueCreationType.SoleQueue;

            return await CreateQueue(createQueueType).CreateQueue(queueName, awsSqsConfiguration);
        }
        
        private IAwsQueueCreator CreateQueue(AwsQueueCreationType awsQueueCreationType)
        {
            return awsQueueCreationType switch
            {
                AwsQueueCreationType.SoleQueue => new AwsSoleQueueCreator(client),
                AwsQueueCreationType.WithErrorQueue => new AwsQueueWithErrorsCreator(client),
                _ => throw new ApplicationException($"Can not find queue creator for: {awsQueueCreationType}")
            };
        }

        //TODO - Update method to update all error queues redrive MaxReceiveCount property
        public async Task<bool> UpdateQueueAttributesAsync(string queueUrl, AwsSqsConfiguration configuration)
        {
            var response = await client.SetQueueAttributesAsync(queueUrl, configuration.QueueAttributes.GetAttributeDictionary());

            return response.HttpStatusCode == HttpStatusCode.OK;
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

            if (queueUrls == null || queueUrls.Count == 0) return [];

            var items = new List<string>();
            foreach (var queueUrl in queueUrls.Where(queueUrl => !string.IsNullOrWhiteSpace(queueUrl) && queueUrl.Contains("/")))
            {
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
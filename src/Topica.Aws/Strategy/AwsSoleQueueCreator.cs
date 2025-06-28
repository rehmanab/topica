using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using Topica.Aws.Contracts;
using Topica.Aws.Helpers;
using Topica.Aws.Queues;

namespace Topica.Aws.Strategy
{
    public class AwsSoleQueueCreator(IAmazonSQS client) : IAwsQueueCreator
    {
        public async Task<string> CreateQueue(string queueName, AwsSqsConfiguration configuration, CancellationToken cancellationToken)
        {
            queueName = configuration.QueueAttributes.IsFifoQueue
                ? TopicQueueHelper.AddTopicQueueNameFifoSuffix(queueName, true)
                : TopicQueueHelper.RemoveTopicQueueNameFifoSuffix(queueName);
            
            var request = new CreateQueueRequest
            {
                QueueName = queueName,
                Attributes = configuration.QueueAttributes.GetAttributeDictionary()
            };

            var response = await client.CreateQueueAsync(request, cancellationToken);

            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                return response.QueueUrl;
            }

            throw new ApplicationException($"Error creating queue, response from AWS: { JsonConvert.SerializeObject(response) }");
        }
    }
}
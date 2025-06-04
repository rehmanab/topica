using System;
using System.Net;
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
        public async Task<string> CreateQueue(string queueName, AwsSqsConfiguration configuration)
        {
            var isFifo = configuration.QueueAttributes.IsFifoQueue;
            if (isFifo) queueName = queueName.Replace(Constants.FifoSuffix, string.Empty);
            
            var request = new CreateQueueRequest
            {
                QueueName = $"{queueName}{(isFifo ? Constants.FifoSuffix : "")}",
                Attributes = configuration.QueueAttributes.GetAttributeDictionary()
            };

            var response = await client.CreateQueueAsync(request);

            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                return response.QueueUrl;
            }

            throw new ApplicationException($"Error creating queue, response from AWS: { JsonConvert.SerializeObject(response) }");
        }
    }
}
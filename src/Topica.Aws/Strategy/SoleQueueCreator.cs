using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using Topica.Aws.Contracts;
using Topica.Aws.Queues;

namespace Topica.Aws.Strategy
{
    public class SoleQueueCreator : IQueueCreator
    {
        private const string FifoQueueSuffix = ".fifo";
        private readonly IAmazonSQS _client;

        public SoleQueueCreator(IAmazonSQS client)
        {
            _client = client;
        }

        public async Task<string> CreateQueue(string queueName, QueueConfiguration? configuration)
        {
            var isFifo = configuration.QueueAttributes.IsFifoQueue;
            if (isFifo) queueName = queueName.Replace(".fifo", string.Empty);
            
            var request = new CreateQueueRequest
            {
                QueueName = $"{queueName}{(isFifo ? FifoQueueSuffix : "")}",
                Attributes = configuration.QueueAttributes.GetAttributeDictionary()
            };

            var response = await _client.CreateQueueAsync(request);

            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                return response.QueueUrl;
            }

            throw new ApplicationException($"Error creating queue, response from AWS: { JsonConvert.SerializeObject(response) }");
        }
    }
}
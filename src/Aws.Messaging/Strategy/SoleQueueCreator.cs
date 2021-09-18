using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Aws.Messaging.Config;
using Aws.Messaging.Contracts;
using Newtonsoft.Json;

namespace Aws.Messaging.Strategy
{
    public class SoleQueueCreator : IQueueCreator
    {
        private readonly IAmazonSQS _client;

        public SoleQueueCreator(IAmazonSQS client)
        {
            _client = client;
        }

        public async Task<string> CreateQueue(string queueName, SqsConfiguration configuration)
        {
            var request = new CreateQueueRequest
            {
                QueueName = queueName,
                Attributes = configuration.QueueAttributes.GetAttributeDictionary()
            };

            var response = await _client.CreateQueueAsync(request);

            if (response.HttpStatusCode == HttpStatusCode.OK) return response.QueueUrl;

            throw new ApplicationException($"Error creating queue, response from AWS: { JsonConvert.SerializeObject(response) }");
        }
    }
}
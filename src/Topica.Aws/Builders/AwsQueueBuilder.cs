using System.Collections.Generic;
using Topica.Aws.Contracts;
using Topica.Aws.Queues;

namespace Topica.Aws.Builders
{
    public class AwsQueueBuilder : IQueueBuilder
    {
        private readonly IAwsQueueService _awsQueueService;

        public AwsQueueBuilder(IAwsQueueService awsQueueService)
        {
            _awsQueueService = awsQueueService;
        }
        
        public IQueueOptionalSettings WithQueueName(string queueName)
        {
            return new AwsQueueOptionalSettings(_awsQueueService, queueName); 
        }

        public IQueueOptionalSettings WithQueueNames(IEnumerable<string> queueNames)
        {
            return new AwsQueueOptionalSettings(_awsQueueService, queueNames); 
        }
    }
}
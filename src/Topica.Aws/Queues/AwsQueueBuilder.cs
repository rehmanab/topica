using System.Collections.Generic;
using Topica.Aws.Contracts;

namespace Topica.Aws.Queues
{
    public class AwsQueueBuilder : IQueueBuilder
    {
        private readonly IQueueProvider _queueProvider;

        public AwsQueueBuilder(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }
        
        public IQueueOptionalSettings WithQueueName(string queueName)
        {
            return new AwsQueueOptionalSettings(_queueProvider, queueName); 
        }

        public IQueueOptionalSettings WithQueueNames(IEnumerable<string> queueNames)
        {
            return new AwsQueueOptionalSettings(_queueProvider, queueNames); 
        }
    }
}
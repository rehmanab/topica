using System;
using Amazon.SQS;
using Topica.Aws.Contracts;
using Topica.Aws.Queues;
using Topica.Aws.Strategy;

namespace Topica.Aws.Factories
{
    public class AwsQueueCreationFactory : IAwsQueueCreationFactory
    {
        private readonly IAmazonSQS _sqsClient;

        public AwsQueueCreationFactory(IAmazonSQS sqsClient)
        {
            _sqsClient = sqsClient;
        }

        public IAwsQueueCreator Create(AwsQueueCreationType awsQueueCreationType)
        {
            switch (awsQueueCreationType)
            {
                case AwsQueueCreationType.SoleQueue:
                    return new AwsSoleQueueCreator(_sqsClient);
                case AwsQueueCreationType.WithErrorQueue:
                    return new AwsQueueWithErrorsCreator(_sqsClient);
                default:
                    throw new ApplicationException($"Can not find queue creator for: {awsQueueCreationType}");
            }
        }
    }
}
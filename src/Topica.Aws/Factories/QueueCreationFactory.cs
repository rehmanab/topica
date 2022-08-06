using System;
using Amazon.SQS;
using Topica.Aws.Contracts;
using Topica.Aws.Queues;
using Topica.Aws.Strategy;

namespace Topica.Aws.Factories
{
    public class QueueCreationFactory : IQueueCreationFactory
    {
        private readonly IAmazonSQS _sqsClient;

        public QueueCreationFactory(IAmazonSQS sqsClient)
        {
            _sqsClient = sqsClient;
        }

        public IQueueCreator Create(QueueCreationType queueCreationType)
        {
            switch (queueCreationType)
            {
                case QueueCreationType.SoleQueue:
                    return new SoleQueueCreator(_sqsClient);
                case QueueCreationType.WithErrorQueue:
                    return new QueueWithErrorsCreator(_sqsClient);
                default:
                    throw new ApplicationException($"Can not find queue creator for: {queueCreationType}");
            }
        }
    }
}
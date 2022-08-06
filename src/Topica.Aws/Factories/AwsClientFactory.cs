using System;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Topica.Aws.Contracts;
using Topica.Aws.Settings;

namespace Topica.Aws.Factories
{
    public class AwsClientFactory : IAwsClientFactory
    {
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly IAmazonSQS _sqsClient;

        public AwsClientFactory(IAmazonSimpleNotificationService snsClient, IAmazonSQS sqsClient)
        {
            _snsClient = snsClient;
            _sqsClient = sqsClient;
        }

        public IAmazonService Create(AwsType type)
        {
            return type switch
            {
                AwsType.Sns => _snsClient,
                AwsType.Sqs => _sqsClient,
                _ => throw new ApplicationException($"Can not find an implementation for {type}")
            };
        }

        public IAmazonSimpleNotificationService GetSnsClient(RegionEndpoint region)
        {
            throw new NotImplementedException();
        }

        public IAmazonSQS GetSqsClient(RegionEndpoint region)
        {
            throw new NotImplementedException();
        }
    }
}
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Aws.Messaging.Settings;

namespace Aws.Messaging.Contracts
{
    public interface IAwsClientFactory
    {
        IAmazonService Create(AwsType type);
        IAmazonSimpleNotificationService GetSnsClient(RegionEndpoint region);
        IAmazonSQS GetSqsClient(RegionEndpoint region);
    }
}
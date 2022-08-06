using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Topica.Aws.Settings;

namespace Topica.Aws.Contracts
{
    public interface IAwsClientFactory
    {
        IAmazonService Create(AwsType type);
        IAmazonSimpleNotificationService GetSnsClient(RegionEndpoint region);
        IAmazonSQS GetSqsClient(RegionEndpoint region);
    }
}
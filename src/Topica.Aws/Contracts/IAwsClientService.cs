using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Topica.Aws.Configuration;

namespace Topica.Aws.Contracts;

public interface IAwsClientService
{
    IAmazonSQS GetSqsClient(AwsTopicaConfiguration awsSettings);
    IAmazonSimpleNotificationService GetSnsClient(AwsTopicaConfiguration awsSettings);
}
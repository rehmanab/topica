using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Microsoft.Extensions.Logging;
using Topica;
using Topica.Aws.Builders;
using Topica.Aws.Configuration;
using Topica.Aws.Contracts;
using Topica.Aws.Factories;
using Topica.Aws.Queues;
using Topica.Aws.Settings;
using Topica.Aws.Topics;
using Topica.Contracts;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AwsServiceExtensions
    {
        public static IServiceCollection AddAwsTopica(this IServiceCollection services, string serviceUrl)
        {
            var logger = services.BuildServiceProvider().GetService<ILogger<MessagingPlatform>>();
            logger.LogDebug("******* AwsServiceExtensions ******* ");

            services.AddScoped<IAmazonSimpleNotificationService>(_ => GetSnsClient(logger, serviceUrl: serviceUrl));
            services.AddScoped<IAmazonSQS>(_ => GetSqsClient(logger, serviceUrl: serviceUrl));
            services.AddScoped<IQueueProvider, AwsQueueProvider>();
            services.AddScoped<ISqsConfigurationBuilder, SqsConfigurationBuilder>();
            services.AddScoped<IQueueCreationFactory, QueueCreationFactory>();
            services.AddScoped<IAwsPolicyBuilder, AwsPolicyBuilder>();
            services.AddScoped<ITopicProvider, AwsTopicProvider>();
            services.AddScoped(_ => new AwsDefaultAttributeSettings
            {
                MaximumMessageSize = 262144, MessageRetentionPeriod = 1209600,
                VisibilityTimeout = 30,
                FifoSettings = new AwsSqsFifoQueueSettings{IsFifoQueue = true, IsContentBasedDeduplication = true}
            });
            services.AddScoped<IAwsTopicBuilder, AwsAwsTopicBuilder>();
            services.AddScoped<IQueueBuilder, AwsQueueBuilder>();
            services.AddScoped<IQueueConsumer, AwsQueueConsumer>();

            return services;
        }
        
        public static IAmazonSQS GetSqsClient(ILogger<MessagingPlatform> logger, string profileName = null, string regionEndpoint = "eu-west-1", string serviceUrl = null)
        {
            var sharedFile = new SharedCredentialsFile();
            var config = new AmazonSQSConfig { RegionEndpoint = RegionEndpoint.GetBySystemName(regionEndpoint) };

            if (string.IsNullOrWhiteSpace(profileName) || !sharedFile.TryGetProfile(profileName, out var profile))
            {
                logger.LogDebug("Using LocalStack");
                config.ServiceURL = serviceUrl;

                return new AmazonSQSClient(new BasicAWSCredentials("", ""), config);
            }

            AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var credentials);

            logger.LogDebug($"Using AWS profile: {profileName}");
            return new AmazonSQSClient(credentials, config);
        }
        
        public static IAmazonSimpleNotificationService GetSnsClient(ILogger<MessagingPlatform> logger, string profileName = null, string regionEndpoint = "eu-west-1", string serviceUrl = null)
        {
            var sharedFile = new SharedCredentialsFile();
            var config = new AmazonSimpleNotificationServiceConfig { RegionEndpoint = RegionEndpoint.GetBySystemName(regionEndpoint) };

            if (string.IsNullOrWhiteSpace(profileName) || !sharedFile.TryGetProfile(profileName, out var profile))
            {
                logger.LogDebug("Using LocalStack");
                config.ServiceURL = serviceUrl;

                return new AmazonSimpleNotificationServiceClient(new BasicAWSCredentials("", ""), config);
            }

            AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var credentials);

            logger.LogDebug($"Using AWS profile: {profileName}");
            return new AmazonSimpleNotificationServiceClient(credentials, config);
        }
    }
}
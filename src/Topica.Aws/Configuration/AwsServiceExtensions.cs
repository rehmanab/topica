using System;
using System.Reflection;
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
using Topica.Executors;
using Topica.Resolvers;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class AwsServiceExtensions
    {
        public static IServiceCollection AddAwsTopica(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            
            var logger = serviceProvider.GetService<ILogger<MessagingPlatform>>();
            logger.LogDebug("******* Aws Service Extensions ******* ");

            var awsSettings = serviceProvider.GetService<AwsSettings>();

            services.AddScoped<IAmazonSimpleNotificationService>(_ => GetSnsClient(logger, awsSettings));
            services.AddScoped<IAmazonSQS>(_ => GetSqsClient(logger, awsSettings));
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
            services.AddScoped<IConsumer, AwsQueueConsumer>();
            
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
            {
                throw new Exception($"{nameof(AwsServiceExtensions)}: entry assembly is null, this can happen if the executing application is from unmanaged code");
            }
            
            services.AddScoped<IHandlerResolver>(_ => new HandlerResolver(services.BuildServiceProvider(), entryAssembly));
            services.AddTransient<IMessageHandlerExecutor, MessageHandlerExecutor>();
            
            // Scan for IHandlers from Entry assembly
            services.Scan(s => s
                .FromAssemblies(entryAssembly!)
                .AddClasses(c => c.AssignableTo(typeof(IHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            return services;
        }
        
        public static IAmazonSQS GetSqsClient(ILogger<MessagingPlatform> logger, AwsSettings awsSettings)
        {
            var sharedFile = new SharedCredentialsFile();
            var config = new AmazonSQSConfig { RegionEndpoint = RegionEndpoint.GetBySystemName(awsSettings.RegionEndpoint) };

            if (!string.IsNullOrWhiteSpace(awsSettings.ProfileName) && sharedFile.TryGetProfile(awsSettings.ProfileName, out var profile))
            {
                AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var credentials);
                logger.LogInformation($"Using AWS profile: {awsSettings.ProfileName} for {nameof(GetSqsClient)}");
                return new AmazonSQSClient(credentials, config);
            }
            
            if (!string.IsNullOrWhiteSpace(awsSettings.AccessKey) && !string.IsNullOrWhiteSpace(awsSettings.SecretKey))
            {
                logger.LogInformation($"Using AccessKey and SecretKey for {nameof(GetSqsClient)}");

                if (!string.IsNullOrEmpty(awsSettings.ServiceUrl))
                {
                    config.ServiceURL = awsSettings.ServiceUrl;
                }
                return new AmazonSQSClient(new BasicAWSCredentials(awsSettings.AccessKey, awsSettings.SecretKey), config);
            }
            
            if (string.IsNullOrEmpty(awsSettings.ServiceUrl))
            {
                throw new Exception($"Please set the ServiceUrl to use localstack for {nameof(GetSqsClient)}");
            }
            
            logger.LogInformation($"Using LocalStack for {nameof(GetSqsClient)}");
            config.ServiceURL = awsSettings.ServiceUrl;
            return new AmazonSQSClient(new BasicAWSCredentials("", ""), config);
        }
        
        public static IAmazonSimpleNotificationService GetSnsClient(ILogger<MessagingPlatform> logger, AwsSettings awsSettings)
        {
            var sharedFile = new SharedCredentialsFile();
            var config = new AmazonSimpleNotificationServiceConfig { RegionEndpoint = RegionEndpoint.GetBySystemName(awsSettings.RegionEndpoint) };

            if (!string.IsNullOrWhiteSpace(awsSettings.ProfileName) && sharedFile.TryGetProfile(awsSettings.ProfileName, out var profile))
            {
                AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var credentials);
                logger.LogInformation($"Using AWS profile: {awsSettings.ProfileName} for {nameof(GetSnsClient)}");
                return new AmazonSimpleNotificationServiceClient(credentials, config);
            }
            
            if (!string.IsNullOrWhiteSpace(awsSettings.AccessKey) && !string.IsNullOrWhiteSpace(awsSettings.SecretKey))
            {
                logger.LogInformation($"Using AccessKey and SecretKey for {nameof(GetSnsClient)}");

                if (!string.IsNullOrEmpty(awsSettings.ServiceUrl))
                {
                    config.ServiceURL = awsSettings.ServiceUrl;
                }
                return new AmazonSimpleNotificationServiceClient(new BasicAWSCredentials(awsSettings.AccessKey, awsSettings.SecretKey), config);
            }
            
            if (string.IsNullOrEmpty(awsSettings.ServiceUrl))
            {
                throw new Exception($"Please set the ServiceUrl to use localstack for {nameof(GetSnsClient)}");
            }
            
            logger.LogInformation($"Using LocalStack for {nameof(GetSnsClient)}");
            config.ServiceURL = awsSettings.ServiceUrl;
            return new AmazonSimpleNotificationServiceClient(new BasicAWSCredentials("", ""), config);
        }
    }
}
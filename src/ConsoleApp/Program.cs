using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Aws.Builders;
using Topica.Aws.Configuration;
using Topica.Aws.Contracts;
using Topica.Aws.Factories;
using Topica.Aws.Queues;
using Topica.Aws.Settings;
using Topica.Aws.Topics;

namespace ConsoleApp
{
    public class Program
    {
        private const string LocalStackServiceUrl = "http://dockerhost:4566";

        private static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var logger = host.Services.GetService<ILogger<Program>>();

            logger.LogInformation("******* Starting Topic creator ******* ");

            var topicBuilder = host.Services.GetService<ITopicBuilder>();
            var queueBuilder = host.Services.GetService<IQueueBuilder>();

            const int incrementNumber = 1;
            
            var topicArn = await topicBuilder
                .WithTopicName($"ar-sns-test-{incrementNumber}")
                .WithSubscribedQueue($"ar-sqs-test-{incrementNumber}_1")
                .WithSubscribedQueue($"ar-sqs-test-{incrementNumber}_2")
                .WithSubscribedQueue($"ar-sqs-test-{incrementNumber}_3")
                .WithQueueConfiguration(host.Services.GetService<ISqsConfigurationBuilder>().BuildCreateWithErrorQueue(5))
                .BuildAsync();
            logger.LogInformation(topicArn);

            var queueUrls = await queueBuilder
                .WithQueueName($"ar-test-2")
                .WithQueueConfiguration(host.Services.GetService<ISqsConfigurationBuilder>().BuildCreateWithErrorQueue(3))
                .BuildAsync();
            logger.LogInformation($"QueueUrls: {string.Join(", ", queueUrls)}");
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                    {
                        builder
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                            .AddEnvironmentVariables();
                    }
                )
                .ConfigureServices(services =>
                {
                    services.AddTransient<IAmazonSimpleNotificationService>(_ => GetSnsClient());
                    services.AddTransient<IAmazonSQS>(_ => GetSqsClient());
                    services.AddTransient<IQueueProvider, AwsQueueProvider>();
                    services.AddTransient<ISqsConfigurationBuilder, SqsConfigurationBuilder>();
                    services.AddTransient<IQueueCreationFactory, QueueCreationFactory>();
                    services.AddTransient<IAwsPolicyBuilder, AwsPolicyBuilder>();
                    services.AddTransient<ITopicProvider, AwsTopicProvider>();
                    services.AddTransient(_ => new AwsDefaultAttributeSettings
                    {
                        MaximumMessageSize = 262144, MessageRetentionPeriod = 1209600,
                        VisibilityTimeout = 30,
                        FifoSettings = new AwsSqsFifoQueueSettings{IsFifoQueue = true, IsContentBasedDeduplication = true}
                    });
                    services.AddTransient<ITopicBuilder, AwsTopicBuilder>();
                    services.AddTransient<IQueueBuilder, AwsQueueBuilder>();
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                });
                
        public static IAmazonSQS GetSqsClient(string profileName = null, string regionEndpoint = "eu-west-1")
        {
            var sharedFile = new SharedCredentialsFile();
            var config = new AmazonSQSConfig { RegionEndpoint = RegionEndpoint.GetBySystemName(regionEndpoint) };

            if (string.IsNullOrWhiteSpace(profileName) || !sharedFile.TryGetProfile(profileName, out var profile))
            {
                Console.WriteLine("Using LocalStack");
                config.ServiceURL = LocalStackServiceUrl;

                return new AmazonSQSClient(new BasicAWSCredentials("", ""), config);
            }

            AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var credentials);

            Console.WriteLine($"Using AWS profile: {profileName}");
            return new AmazonSQSClient(credentials, config);
        }
        
        public static IAmazonSimpleNotificationService GetSnsClient(string profileName = null, string regionEndpoint = "eu-west-1")
        {
            var sharedFile = new SharedCredentialsFile();
            var config = new AmazonSimpleNotificationServiceConfig { RegionEndpoint = RegionEndpoint.GetBySystemName(regionEndpoint) };

            if (string.IsNullOrWhiteSpace(profileName) || !sharedFile.TryGetProfile(profileName, out var profile))
            {
                Console.WriteLine("Using LocalStack");
                config.ServiceURL = LocalStackServiceUrl;

                return new AmazonSimpleNotificationServiceClient(new BasicAWSCredentials("", ""), config);
            }

            AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var credentials);

            Console.WriteLine($"Using AWS profile: {profileName}");
            return new AmazonSimpleNotificationServiceClient(credentials, config);
        }
    }
}
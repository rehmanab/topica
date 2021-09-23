using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Aws.Messaging.Builders;
using Aws.Messaging.Contracts;
using Aws.Messaging.Factories;
using Aws.Messaging.Notifications;
using Aws.Messaging.Queue;
using Aws.Messaging.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var logger = host.Services.GetService<ILogger<Program>>();

            logger.LogInformation("******* Starting Topic creator ******* ");

            var topicBuilder = host.Services.GetService<ITopicBuilder>();
            var queueBuilder = host.Services.GetService<IQueueBuilder>();

            const int incrementNumber = 1;
            
            // var topicArn = await topicBuilder
            //     .WithTopicName($"ar-sns-test-{incrementNumber}")
            //     .WithSubscribedQueue($"ar-sqs-test-{incrementNumber}_1")
            //     .WithSubscribedQueue($"ar-sqs-test-{incrementNumber}_2")
            //     .WithSubscribedQueue($"ar-sqs-test-{incrementNumber}_3")
            //     .WithQueueConfiguration(host.Services.GetService<ISqsConfigurationBuilder>().BuildCreateWithErrorQueue(5))
            //     .BuildAsync();
            // logger.LogInformation(topicArn);

            var queueUrls = await queueBuilder
                .WithQueueName($"ar-sqs-test-{incrementNumber}")
                .WithQueueConfiguration(host.Services.GetService<ISqsConfigurationBuilder>().BuildCreateWithErrorQueue(5))
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
                    var awsRegion = RegionEndpoint.GetBySystemName("eu-west-1");
                    services.AddTransient<IAmazonSimpleNotificationService>(_ => new AmazonSimpleNotificationServiceClient(awsRegion));
                    services.AddTransient<IAmazonSQS>(_ => new AmazonSQSClient(awsRegion));
                    services.AddTransient<IQueueProvider, QueueProvider>();
                    services.AddTransient<ISqsConfigurationBuilder, SqsConfigurationBuilder>();
                    services.AddTransient<IQueueCreationFactory, QueueCreationFactory>();
                    services.AddTransient<IAwsPolicyBuilder, AwsPolicyBuilder>();
                    services.AddTransient<ITopicProvider, TopicProvider>();
                    services.AddTransient(_ => new AwsDefaultAttributeSettings
                    {
                        MaximumMessageSize = 262144, MessageRetentionPeriod = 1209600, FifoQueue = true,
                        VisibilityTimeout = 30
                    });
                    services.AddTransient<ITopicBuilder, TopicBuilder>();
                    services.AddTransient<IQueueBuilder, QueueBuilder>();
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                });
    }
}
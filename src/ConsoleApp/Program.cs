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
using Aws.Messaging.Queue.SQS;
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

            logger.LogInformation("******* INFO ******* HERE");

            var topicCreator = host.Services.GetService<ITopicCreator>();
            
            var topicArn = topicCreator
                .WithTopicName("ar-sns-test-15")
                .WithSubscribedQueue("ar-sqs-test-15_1")
                .WithSubscribedQueue("ar-sqs-test-15_2")
                .WithQueueConfiguration(host.Services.GetService<ISqsConfigurationBuilder>().BuildCreateWithErrorQueue(5))
                .Create();
            
            logger.LogInformation(topicArn);
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
                    services.AddTransient<INotificationProvider, NotificationProvider>();
                    services.AddTransient(_ => new AwsDefaultAttributeSettings
                    {
                        MaximumMessageSize = 262144, MessageRetentionPeriod = 1209600, FifoQueue = true
                    });
                    services.AddTransient<ITopicCreator, TopicCreator>();
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                });
    }
}
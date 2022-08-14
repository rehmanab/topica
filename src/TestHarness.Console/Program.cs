using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica;
using Topica.Aws.Configuration;
using Topica.Aws.Settings;
using Topica.Aws.Topics;
using Topica.Contracts;
using Topica.Kafka.Configuration;
using Topica.Kafka.Settings;
using Topica.Kafka.Topics;
using Topica.Topics;

namespace TestHarness.Console
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var logger = host.Services.GetService<ILogger<Program>>();

            logger.LogInformation("******* Starting Topic creator ******* ");

            var topicCreatorFactory = host.Services.GetService<ITopicCreatorFactory>();

            await AwsCreateTopicAndConsume($"ar-orders_{4}", 2, topicCreatorFactory);
            await AwsCreateTopicAndConsume($"ar-customers_{4}", 2, topicCreatorFactory);
            // var topicArn = await KafkaCreateTopic(topicCreatorFactory);
            
            logger.LogInformation("******* Created Topics");
        }

        public static async Task AwsCreateTopicAndConsume(string sourceName, int numberOfInstances, ITopicCreatorFactory topicCreatorFactory)
        {
            var topicCreator = topicCreatorFactory!.Create(MessagingPlatform.Aws);
            var consumer = await topicCreator.CreateTopic(new AwsTopicSettings
            {
                TopicName = sourceName,
                WithSubscribedQueues = new List<string>
                {
                    sourceName
                },
                BuildWithErrorQueue = true,
                ErrorQueueMaxReceiveCount = 10,
                VisibilityTimeout = 30,
                IsFifoQueue = true,
                IsFifoContentBasedDeduplication = true
            });
            
            // var queueUrls = await queueBuilder
            //     .WithQueueName($"ar-test-2")
            //     .WithQueueConfiguration(host.Services.GetService<ISqsConfigurationBuilder>().BuildCreateWithErrorQueue(3))
            //     .BuildAsync();
            // logger.LogInformation($"QueueUrls: {string.Join(", ", queueUrls)}");

            await Task.CompletedTask;
        }
        
        public static async Task<IConsumer> KafkaCreateTopic(ITopicCreatorFactory topicCreatorFactory)
        {
            var topicCreator = topicCreatorFactory!.Create(MessagingPlatform.Kafka);
            var consumer = await topicCreator.CreateTopic(new KafkaTopicSettings
            {
                TopicName = "ar-kafka-test-1",
                NumberOfPartitions = 10
            });

            return consumer;
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                    {
                        builder
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                            .AddEnvironmentVariables();
                    }
                )
                .ConfigureServices(services =>
                {
                    // Configuration
                    services.AddSingleton(provider =>
                    {
                        var config = provider.GetRequiredService<IConfiguration>();
                        return config.GetSection(AwsSettings.SectionName).Get<AwsSettings>();
                    });
                    services.AddSingleton(provider =>
                    {
                        var config = provider.GetRequiredService<IConfiguration>();
                        return config.GetSection(KafkaSettings.SectionName).Get<KafkaSettings>();
                    });
                    
                    // Add Topica main dependencies
                    services.AddScoped<ITopicCreatorFactory, TopicCreatorFactory>();
                    services.AddScoped<ITopicCreator, AwsTopicCreator>();
                    services.AddScoped<ITopicCreator, KafkaTopicCreator>();

                    services.AddAwsTopica();
                    services.AddKafkaTopica();
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                });
                
        
    }
}
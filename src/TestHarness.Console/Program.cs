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

            var topicArn = await AwsCreateTopic($"ar-orders_{1}", topicCreatorFactory);
            topicArn += ", " + await AwsCreateTopic($"ar-customers_{1}", topicCreatorFactory);
            // var topicArn = await KafkaCreateTopic(topicCreatorFactory);
            
            logger.LogInformation($"******* Created Topic: {topicArn}");
        }

        public static async Task<string> AwsCreateTopic(string sourceName, ITopicCreatorFactory topicCreatorFactory)
        {
            const int incrementNumber = 1;
            
            var topicCreator = topicCreatorFactory!.Create(MessagingPlatform.Aws);
            var topicArn = await topicCreator.CreateTopic(new AwsTopicConfiguration
            {
                TopicName = sourceName,
                WithSubscribedQueues = new List<string>
                {
                    sourceName
                },
                BuildWithErrorQueue = true,
                ErrorQueueMaxReceiveCount = 10
            });
            
            // var queueUrls = await queueBuilder
            //     .WithQueueName($"ar-test-2")
            //     .WithQueueConfiguration(host.Services.GetService<ISqsConfigurationBuilder>().BuildCreateWithErrorQueue(3))
            //     .BuildAsync();
            // logger.LogInformation($"QueueUrls: {string.Join(", ", queueUrls)}");

            return topicArn;
        }
        
        public static async Task<string> KafkaCreateTopic(ITopicCreatorFactory topicCreatorFactory)
        {
            var topicCreator = topicCreatorFactory!.Create(MessagingPlatform.Kafka);
            var topicArn = await topicCreator.CreateTopic(new KafkaTopicConfiguration
            {
                TopicName = "ar-kafka-test-1",
                NumberOfPartitions = 10
            });

            return topicArn;
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
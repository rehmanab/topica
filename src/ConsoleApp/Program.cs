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
using Topica.Aws.Topics;
using Topica.Contracts;
using Topica.Kafka.Topics;
using Topica.Topics;

namespace ConsoleApp
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var logger = host.Services.GetService<ILogger<Program>>();

            logger.LogInformation("******* Starting Topic creator ******* ");

            var topicCreatorFactory = host.Services.GetService<ITopicCreatorFactory>();
            
            const MessagingPlatform messagingPlatform = MessagingPlatform.Aws;
            var topicArn = await CreateTopic(messagingPlatform, topicCreatorFactory);
            
            logger.LogInformation($"******* Created Topic: {topicArn}");
        }

        public static async Task<string> CreateTopic(MessagingPlatform messagingPlatform, ITopicCreatorFactory topicCreatorFactory)
        {
            const int incrementNumber = 1;
            
            var topicCreator = topicCreatorFactory!.Create(messagingPlatform);
            var topicArn = await topicCreator.CreateTopic(new AwsTopicConfiguration
            {
                TopicName = $"ar-sns-test-{incrementNumber}",
                WithSubscribedQueues = new List<string>
                {
                    $"ar-sqs-test-{incrementNumber}_1",
                    $"ar-sqs-test-{incrementNumber}_2",
                    $"ar-sqs-test-{incrementNumber}_3",
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
                    // Add Topica main dependencies
                    services.AddScoped<ITopicCreatorFactory, TopicCreatorFactory>();
                    services.AddScoped<ITopicCreator, AwsTopicCreator>();
                    services.AddScoped<ITopicCreator, KafkaTopicCreator>();

                    // services.AddAwsTopica("http://dockerhost:4566");
                    // services.AddKafkaTopica("http://dockerhost:4566");
                    
                    // services.AddScoped<ITopicCreator, Kaf>();

                    // services.Scan(s => s
                    //     .FromAssemblies(Assembly.GetExecutingAssembly())
                    //     .AddClasses(c => c.AssignableTo(typeof(ITopicCreator)))
                    //     .AsImplementedInterfaces()
                    //     .WithTransientLifetime());



                })
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                });
                
        
    }
}
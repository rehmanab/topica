using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
using Topica;
using Topica.Aws.Builders;
using Topica.Aws.Configuration;
using Topica.Aws.Contracts;
using Topica.Aws.Factories;
using Topica.Aws.Queues;
using Topica.Aws.Settings;
using Topica.Aws.Topics;
using Topica.Contracts;
using Topica.Kafka.Configuration;
using Topica.Kafka.Topics;
using Topica.Topics;

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

            const int incrementNumber = 3;
            
            var topicCreatorFactory = host.Services.GetService<ITopicCreatorFactory>();
            var topicCreator = topicCreatorFactory!.Create(MessagingPlatform.Aws);
            var topicArn = await topicCreator.CreateTopic(new AwsTopicConfiguration
            {
                TopicName = $"ar-sns-test-{incrementNumber}",
                WithSubscribedQueues = new List<string>
                {
                    $"ar-sqs-test-{incrementNumber}_1",
                    $"ar-sqs-test-{incrementNumber}_2",
                    $"ar-sqs-test-{incrementNumber}_3",
                    $"ar-sqs-test-{incrementNumber}_4",
                    $"ar-sqs-test-{incrementNumber}_5",
                    $"ar-sqs-test-{incrementNumber}_6",
                },
                BuildWithErrorQueue = true,
                ErrorQueueMaxReceiveCount = 10
            });
            logger.LogInformation($"******* Created Topic: {topicArn}");
        
            // var queueUrls = await queueBuilder
            //     .WithQueueName($"ar-test-2")
            //     .WithQueueConfiguration(host.Services.GetService<ISqsConfigurationBuilder>().BuildCreateWithErrorQueue(3))
            //     .BuildAsync();
            // logger.LogInformation($"QueueUrls: {string.Join(", ", queueUrls)}");
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
                    // Add Topica main dependencies
                    services.AddScoped<ITopicCreatorFactory, TopicCreatorFactory>();
                    services.AddScoped<ITopicCreator, AwsTopicCreator>();
                    services.AddScoped<ITopicCreator, KafkaTopicCreator>();

                    services.AddAwsTopica(LocalStackServiceUrl);
                    
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
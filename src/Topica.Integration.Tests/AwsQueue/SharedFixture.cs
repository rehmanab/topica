using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testcontainers.LocalStack;
using Topica.Aws.Contracts;
using Topica.Aws.Helpers;
using Topica.Contracts;
using Topica.Integration.Tests.AwsQueue.Settings;
using Xunit;

namespace Topica.Integration.Tests.AwsQueue;

public class SharedFixture : IAsyncLifetime
{
    private LocalStackContainer _localStackContainer = null!;
    private readonly string _queueSuffix = Guid.NewGuid().ToString();
    
    public static int DelaySeconds => 3;

    private string ConsumerQueueName { get; set; } = null!;
    public string ProducerQueueName { get; private set; } = null!;

    private IAwsQueueService AwsQueueService { get; set; } = null!;

    public IConsumer Consumer { get; private set; } = null!;
    public IProducer Producer { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // Localstack
        _localStackContainer = new LocalStackBuilder()
            //.WithAutoRemove(true)
            //.WithReuse(true)
            .WithName("localstack")
            .WithImage("localstack/localstack:4.4.0")
            .Build();
        
        await _localStackContainer.StartAsync();
        var localStackServiceUrl = _localStackContainer.GetConnectionString();
        
        // Consumer
        var awsQueueConsumerHost = AwsQueueConsumerConfigureServices(localStackServiceUrl).Build();
        Assert.NotNull(awsQueueConsumerHost);

        var awsConsumerQueueBuilder = awsQueueConsumerHost.Services.GetService<IAwsQueueBuilder>();
        Assert.NotNull(awsConsumerQueueBuilder);
        
        Consumer = await awsConsumerQueueBuilder.BuildConsumerAsync(CancellationToken.None);
        
        // Producer
        var awsQueueProducerHost = AwsQueueProducerConfigureServices(localStackServiceUrl).Build();
        Assert.NotNull(awsQueueProducerHost);
        
        var awsProducerQueueBuilder = awsQueueProducerHost.Services.GetService<IAwsQueueBuilder>();
        Assert.NotNull(awsProducerQueueBuilder);
        
        Producer = await awsProducerQueueBuilder.BuildProducerAsync(CancellationToken.None);
        Assert.NotNull(Producer);
        
        Assert.Equal(ConsumerQueueName, ProducerQueueName);
        
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await Consumer.DisposeAsync();
        await Producer.DisposeAsync();

        await _localStackContainer.DisposeAsync();

        // Not needed to delete topics, queues when using TestContainers, as container will be disposed
        // var mainQueueDeleted =  AwsQueueService.DeleteQueueAsync(await AwsQueueService.GetQueueUrlAsync(ConsumerQueueName, CancellationToken.None), CancellationToken.None);
        // var errorQueueName = TopicQueueHelper.AddTopicQueueNameErrorAndFifoSuffix(ConsumerQueueName, ConsumerQueueName.EndsWith(Constants.FifoSuffix));
        // var errorQueueDeleted =  AwsQueueService.DeleteQueueAsync(await AwsQueueService.GetQueueUrlAsync(errorQueueName, CancellationToken.None), CancellationToken.None);
    }

    private IHostBuilder AwsQueueConsumerConfigureServices(string serviceUrl)
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(builder =>
                {
                    builder
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("AwsQueue/appsettings.json", optional: false, reloadOnChange: true);
                }
            )
            .ConfigureServices(services =>
            {
                services.AddLogging(configure => configure
                    .AddFilter("*", LogLevel.Debug)
                    .AddSimpleConsole(x =>
                    {
                        x.IncludeScopes = false;
                        x.TimestampFormat = "[HH:mm:ss] ";
                        x.SingleLine = true;
                    }));

                // Configuration
                var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                var hostSettings = configuration.GetSection(AwsHostSettings.SectionName).Get<AwsHostSettings>();
                var settings = configuration.GetSection(AwsConsumerSettings.SectionName).Get<AwsConsumerSettings>();

                if (hostSettings == null) throw new InvalidOperationException($"{nameof(AwsHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
                if (settings == null) throw new InvalidOperationException($"{nameof(AwsConsumerSettings)} is not configured. Please check your appsettings.json or environment variables.");

                services.AddSingleton(hostSettings);
                services.AddSingleton(settings);

                // Add MessagingPlatform Components
                services.AddAwsTopica(c =>
                {
                    c.ProfileName = hostSettings.ProfileName;
                    c.AccessKey = hostSettings.AccessKey;
                    c.SecretKey = hostSettings.SecretKey;
                    c.ServiceUrl = serviceUrl;
                    c.RegionEndpoint = hostSettings.RegionEndpoint;
                // }, Assembly.GetAssembly(typeof(ClassToReferenceAssembly)) ?? throw new InvalidOperationException());
                }, Assembly.GetExecutingAssembly());

                // Creation Builder
                var consumerQueueName = TopicQueueHelper.AddTopicQueueNameFifoSuffix($"{settings.WebAnalyticsQueueSettings.Source}_{_queueSuffix}", settings.WebAnalyticsQueueSettings.IsFifoQueue ?? false);
                services.AddSingleton(services.BuildServiceProvider().GetRequiredService<IAwsQueueCreationBuilder>()
                    .WithWorkerName(settings.WebAnalyticsQueueSettings.WorkerName)
                    .WithQueueName(consumerQueueName)
                    .WithErrorQueueSettings(
                        settings.WebAnalyticsQueueSettings.BuildWithErrorQueue,
                        settings.WebAnalyticsQueueSettings.ErrorQueueMaxReceiveCount
                    )
                    .WithFifoSettings(
                        settings.WebAnalyticsQueueSettings.IsFifoQueue,
                        settings.WebAnalyticsQueueSettings.IsFifoContentBasedDeduplication
                    )
                    .WithTemporalSettings(
                        settings.WebAnalyticsQueueSettings.MessageVisibilityTimeoutSeconds,
                        settings.WebAnalyticsQueueSettings.QueueMessageDelaySeconds,
                        settings.WebAnalyticsQueueSettings.QueueMessageRetentionPeriodSeconds,
                        settings.WebAnalyticsQueueSettings.QueueReceiveMessageWaitTimeSeconds
                    )
                    .WithQueueSettings(settings.WebAnalyticsQueueSettings.QueueMaximumMessageSize)
                    .WithConsumeSettings(
                        settings.WebAnalyticsQueueSettings.NumberOfInstances,
                        settings.WebAnalyticsQueueSettings.QueueReceiveMaximumNumberOfMessages
                    ));
                
                // Custom for these integration tests
                ConsumerQueueName = consumerQueueName;
                AwsQueueService = services.BuildServiceProvider().GetRequiredService<IAwsQueueService>();
            });
    }

    private IHostBuilder AwsQueueProducerConfigureServices(string serviceUrl)
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(builder =>
                {
                    builder
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("AwsQueue/appsettings.json", optional: false, reloadOnChange: true);
                }
            )
            .ConfigureServices(services =>
            {
                services.AddLogging(configure => configure.AddSimpleConsole(x =>
                {
                    x.IncludeScopes = false;
                    x.TimestampFormat = "[HH:mm:ss] ";
                    x.SingleLine = true;
                }));

                // Configuration
                var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                var hostSettings = configuration.GetSection(AwsHostSettings.SectionName).Get<AwsHostSettings>();
                var settings = configuration.GetSection(AwsProducerSettings.SectionName).Get<AwsProducerSettings>();

                if (hostSettings == null) throw new InvalidOperationException($"{nameof(AwsHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
                if (settings == null) throw new InvalidOperationException($"{nameof(AwsProducerSettings)} is not configured. Please check your appsettings.json or environment variables.");

                services.AddSingleton(hostSettings);
                services.AddSingleton(settings);

                // Add MessagingPlatform Components
                services.AddAwsTopica(c =>
                {
                    c.ProfileName = hostSettings.ProfileName;
                    c.AccessKey = hostSettings.AccessKey;
                    c.SecretKey = hostSettings.SecretKey;
                    c.ServiceUrl = serviceUrl;
                    c.RegionEndpoint = hostSettings.RegionEndpoint;
                }, Assembly.GetExecutingAssembly());

                // Creation Builder
                var producerQueueName = TopicQueueHelper.AddTopicQueueNameFifoSuffix($"{settings.WebAnalyticsQueueSettings.Source}_{_queueSuffix}", settings.WebAnalyticsQueueSettings.IsFifoQueue ?? false);
                
                services.AddSingleton(services.BuildServiceProvider().GetRequiredService<IAwsQueueCreationBuilder>()
                    .WithWorkerName(settings.WebAnalyticsQueueSettings.WorkerName)
                    .WithQueueName(producerQueueName)
                    .WithErrorQueueSettings(
                        settings.WebAnalyticsQueueSettings.BuildWithErrorQueue,
                        settings.WebAnalyticsQueueSettings.ErrorQueueMaxReceiveCount
                    )
                    .WithFifoSettings(
                        settings.WebAnalyticsQueueSettings.IsFifoQueue,
                        settings.WebAnalyticsQueueSettings.IsFifoContentBasedDeduplication
                    )
                    .WithTemporalSettings(
                        settings.WebAnalyticsQueueSettings.MessageVisibilityTimeoutSeconds,
                        settings.WebAnalyticsQueueSettings.QueueMessageDelaySeconds,
                        settings.WebAnalyticsQueueSettings.QueueMessageRetentionPeriodSeconds,
                        settings.WebAnalyticsQueueSettings.QueueReceiveMessageWaitTimeSeconds
                    )
                    .WithQueueSettings(settings.WebAnalyticsQueueSettings.QueueMaximumMessageSize)
                    .WithConsumeSettings(
                        null,
                        settings.WebAnalyticsQueueSettings.QueueReceiveMaximumNumberOfMessages
                    ));
                
                // Custom for these integration tests
                ProducerQueueName = producerQueueName;
                AwsQueueService = services.BuildServiceProvider().GetRequiredService<IAwsQueueService>();
            });
    }
}
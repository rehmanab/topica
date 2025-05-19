using System.Reflection;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Aws.Producer.Host.Messages.V1;
using Aws.Producer.Host.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RandomNameGeneratorLibrary;
using Topica.Aws.Contracts;
using Topica.Contracts;
using Topica.Settings;

Console.WriteLine("******* Starting Aws.Producer.Host *******");

var host = Host.CreateDefaultBuilder()
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
        services.AddLogging(configure => configure.AddSimpleConsole(x =>
        {
            x.IncludeScopes = false;
            x.TimestampFormat = "[HH:mm:ss] ";
            x.SingleLine = true;
        }));
        
        // Configuration
        var hostSettings = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        var awsHostSettings = hostSettings.GetSection(AwsHostSettings.SectionName).Get<AwsHostSettings>();

        if (awsHostSettings == null)
        {
            throw new ApplicationException("AwsHostSettings not found");
        }
        
        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            return config.GetSection(ProducerSettings.SectionName).Get<ProducerSettings>() ?? throw new InvalidOperationException("ConsumerSettings not found");
        });

        // Add MessagingPlatform Components
        services.AddAwsTopica(c =>
        {
            c.ProfileName = awsHostSettings.ProfileName;
            c.AccessKey = awsHostSettings.AccessKey;
            c.SecretKey = awsHostSettings.SecretKey;
            c.ServiceUrl = awsHostSettings.ServiceUrl;
            c.RegionEndpoint = awsHostSettings.RegionEndpoint;
        }, Assembly.GetExecutingAssembly());
    })
    .Build();

var cts = new CancellationTokenSource();

var producerSettings = host.Services.GetService<ProducerSettings>();
var producerBuilder = host.Services.GetService<IProducerBuilder>() ?? throw new InvalidOperationException("Aws IAmazonSimpleNotificationService not found");
var awsTopicService = host.Services.GetService<IAwsTopicService>();
var producer = await producerBuilder.BuildProducerAsync<IAmazonSimpleNotificationService>("aws_sns_producer-1", producerSettings, cts.Token);
var topicArns = awsTopicService!.GetAllTopics(producerSettings?.Source, producerSettings?.AwsIsFifoQueue).ToBlockingEnumerable().SelectMany(x => x).ToList();

switch (topicArns.Count)
{
    case 0:
        throw new Exception($"No topic found for prefix: {producerSettings?.Source}");
    case > 1:
        throw new Exception($"More than 1 topic found for prefix: {producerSettings?.Source}");
}

var topic = topicArns.First().TopicArn;

var count = 1;
while(true)
{
    var message = new OrderPlacedMessageV1{ConversationId = Guid.NewGuid(), OrderId = count, OrderName = Random.Shared.GenerateRandomMaleFirstAndLastName(), Type = nameof(OrderPlacedMessageV1)};
    var request = new PublishRequest
    {
        TopicArn = topic, 
        Message = JsonConvert.SerializeObject(message),
        MessageAttributes = new Dictionary<string, MessageAttributeValue>
        {
            {
                "SignatureVersion", new MessageAttributeValue { StringValue = "2", DataType = "String"} 
            }
        }
    };
    
    if (topic.EndsWith(".fifo"))
    {
        request.MessageGroupId = Guid.NewGuid().ToString();
        request.MessageDeduplicationId = Guid.NewGuid().ToString();
    }
    
    await producer.PublishAsync(request);
    
    count++;
    
    Console.WriteLine($"Produced message to {producerSettings?.Source}: {count}");
    
    await Task.Delay(1000);
}

producer.Dispose();

Console.WriteLine($"Finished: {count} messages sent.");



// await host.RunAsync();
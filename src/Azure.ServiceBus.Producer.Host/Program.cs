using System.Reflection;
using System.Text;
using Azure.Messaging.ServiceBus;
using Azure.ServiceBus.Producer.Host.Messages.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RandomNameGeneratorLibrary;
using Topica.Azure.ServiceBus.Contracts;
using Topica.Azure.ServiceBus.Settings;
using Topica.Contracts;
using Topica.Settings;

Console.WriteLine("******* Starting Azure.ServiceBus.Producer.Host *******");

var host = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration(builder =>
        {
            builder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .AddEnvironmentVariables();
#if DEBUG
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
#endif
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
        var azureServiceBusHostSettings = hostSettings.GetSection(AzureServiceBusHostSettings.SectionName).Get<AzureServiceBusHostSettings>();
        if (azureServiceBusHostSettings == null || string.IsNullOrEmpty(azureServiceBusHostSettings.ConnectionString))
        {
            throw new ApplicationException("AzureServiceBusHostSettings not found or ConnectionString is empty");
        }
        services.AddSingleton(azureServiceBusHostSettings);
        
        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            return config.GetSection(ProducerSettings.SectionName).Get<ProducerSettings>() ?? throw new InvalidOperationException("ConsumerSettings not found");
        });

        // Add MessagingPlatform Components
        services.AddAzureServiceBusTopica(c =>
        {
            c.ConnectionString = azureServiceBusHostSettings.ConnectionString;
        }, Assembly.GetExecutingAssembly());
        
        services.Configure<HostOptions>(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromSeconds(5);
        });
    })
    .Build();

var cts = new CancellationTokenSource();

var hostSettings = host.Services.GetService<AzureServiceBusHostSettings>();
var producerSettings = host.Services.GetService<ProducerSettings>();
producerSettings!.ConnectionString = hostSettings!.ConnectionString;
var producerBuilder = host.Services.GetService<IProducerBuilder>() ?? throw new InvalidOperationException("AzureServiceBusProducerBuilder not found");
const string consumerName = "azure_service_bus_producer_host_1";
var producer = await producerBuilder.BuildProducerAsync<IServiceBusClientProvider>(consumerName, producerSettings, cts.Token);
var sender = producer.GetServiceBusClient().CreateSender("ar_price_submitted_v1", new ServiceBusSenderOptions { Identifier = consumerName });

var theMessage = JsonConvert.SerializeObject(new PriceSubmittedMessageV1
{
    PriceId = 1234L,
    PriceName = "Some Price",
    ConversationId = Guid.NewGuid(),
    Type = nameof(PriceSubmittedMessageV1),
    RaisingComponent = consumerName,
    Version = "V1",
    AdditionalProperties = new Dictionary<string, string> { { "prop1", "value1" } }
});

var count = 1;
while (true)
{
    var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(theMessage))
    {
        MessageId = Guid.NewGuid().ToString() // MessageId is or can be used for deduplication
    };
    //message.ApplicationProperties.Add("userProp1", "value1");

    await sender.SendMessageAsync(message);
    Console.WriteLine($"Sent: {count}");
    count++;

    await Task.Delay(1000);
}

// await host.RunAsync();
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulsar.Client.Api;
using Pulsar.Producer.Host.Messages.V1;
using Pulsar.Producer.Host.Settings;
using Topica;
using Topica.Contracts;
using Topica.Settings;

Console.WriteLine("******* Starting Pulsar.Producer.Host *******");

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
        var pulsarHostSettings = hostSettings.GetSection(PulsarHostSettings.SectionName).Get<PulsarHostSettings>();

        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            return config.GetSection(ProducerSettings.SectionName).Get<ProducerSettings>() ?? throw new InvalidOperationException("ConsumerSettings not found");
        });

        // Add MessagingPlatform Components
        services.AddPulsarTopica(c =>
        {
            c.ServiceUrl = pulsarHostSettings.ServiceUrl;
            c.PulsarManagerBaseUrl = pulsarHostSettings.PulsarManagerBaseUrl;
            c.PulsarAdminBaseUrl = pulsarHostSettings.PulsarAdminBaseUrl;
        });
    })
    .Build();

var cts = new CancellationTokenSource();

var producerSettings = host.Services.GetService<ProducerSettings>();
var topicProviderFactory = host.Services.GetService<ITopicProviderFactory>();

var topicProvider = topicProviderFactory.Create(MessagingPlatform.Pulsar);
var producerBuilder = await topicProvider.CreateTopicAsync(producerSettings);
var producer = await producerBuilder.BuildProducerAsync<IProducer<byte[]>>("test-producer-1", producerSettings, cts.Token);

var message = new MatchStartedMessage{ConversationId = Guid.NewGuid(), Name = "Stoke Poges"};
foreach (var index in Enumerable.Range(1, 10))
{
    await producer.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
}

await producer.DisposeAsync();

Console.WriteLine("Finished!");



// await host.RunAsync();
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMq.Producer.Host.Messages.V1;
using RabbitMq.Producer.Host.Settings;
using Topica.Contracts;
using Topica.Settings;

Console.WriteLine("******* Starting RabbitMq.Producer.Host *******");

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
        var rabbitMqHostSettings = hostSettings.GetSection(RabbitMqHostSettings.SectionName).Get<RabbitMqHostSettings>() ?? throw new InvalidOperationException("RabbitMqHostSettings not found");

        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            return config.GetSection(ProducerSettings.SectionName).Get<ProducerSettings>() ?? throw new InvalidOperationException("ConsumerSettings not found");
        });

        // Add MessagingPlatform Components
        services.AddRabbitMqTopica(c =>
        {
            c.Hostname = rabbitMqHostSettings.Hostname;
            c.UserName = rabbitMqHostSettings.UserName;
            c.Password = rabbitMqHostSettings.Password;
            c.Scheme = rabbitMqHostSettings.Scheme;
            c.Port = rabbitMqHostSettings.Port;
            c.ManagementPort = rabbitMqHostSettings.ManagementPort;
            c.ManagementScheme = rabbitMqHostSettings.ManagementScheme;
            c.VHost = rabbitMqHostSettings.VHost;
        }, Assembly.GetExecutingAssembly());
    })
    .Build();

var cts = new CancellationTokenSource();

var producerSettings = host.Services.GetService<ProducerSettings>() ?? throw new InvalidOperationException("RabbitMq ProducerSettings not found");
var producerBuilder = host.Services.GetService<IProducerBuilder>() ?? throw new InvalidOperationException("RabbitMq ProducerBuilder not found");

var producer = await producerBuilder.BuildProducerAsync<IChannel>(null, producerSettings, cts.Token);

foreach (var index in Enumerable.Range(1, 1))
{
    var message = new ItemDeliveredMessageV1{ConversationId = Guid.NewGuid(), ItemId = 123L, ItemName = "Rubicon Mango", Type = nameof(ItemDeliveredMessageV1)};
    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
    await producer.BasicPublishAsync("item_delivered_v1_exchange", "", body);
    
    Console.WriteLine($"Produced message to {producerSettings.Source}: {message.ConversationId}");
    
    await Task.Delay(250);
}

producer.Dispose();

Console.WriteLine("Finished!");

// await host.RunAsync();
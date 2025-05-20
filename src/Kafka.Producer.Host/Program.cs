using System.Reflection;
using Confluent.Kafka;
using Kafka.Producer.Host.Messages.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RandomNameGeneratorLibrary;
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
        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            return config.GetSection(ProducerSettings.SectionName).Get<ProducerSettings>() ?? throw new InvalidOperationException("ConsumerSettings not found");
        });

        // Add MessagingPlatform Components
        services.AddKafkaTopica(Assembly.GetExecutingAssembly());
        
        services.Configure<HostOptions>(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromSeconds(5);
        });
    })
    .Build();

var cts = new CancellationTokenSource();

var producerSettings = host.Services.GetService<ProducerSettings>();

if (producerSettings == null)
{
    throw new ApplicationException("ProducerSettings not found");
}

var producerBuilder = host.Services.GetService<IProducerBuilder>() ?? throw new InvalidOperationException("KafkaProducerBuilder not found");
var producer = await producerBuilder.BuildProducerAsync<IProducer<string, string>>("kafka_producer_host_1", producerSettings, cts.Token);

var count = 1;
var personNameGenerator = new PersonNameGenerator();
while(true)
{
    var name = personNameGenerator.GenerateRandomFirstAndLastName();
    var result = await producer.ProduceAsync(producerSettings?.Source, new Message<string, string>
    {
        Key = name,
        Value = JsonConvert.SerializeObject(new PersonCreatedMessageV1 { PersonId = count, PersonName = name, ConversationId = Guid.NewGuid(), Type = nameof(PersonCreatedMessageV1) })
    }, cts.Token);
    // FLush will wait for all messages sent to be confirmed/delivered, so dont use after each Produce()
    //producer.Flush(TimeSpan.FromSeconds(2));

    count++;
    
    Console.WriteLine($"Produced message to {producerSettings?.Source}: {count}");
    
    await Task.Delay(1000);
}

producer.Dispose();

// await host.RunAsync();
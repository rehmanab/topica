using System.Reflection;
using System.Text;
using Aws.Producer.Host.Settings;
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

// var producerSettings = host.Services.GetService<ProducerSettings>();
// var producerBuilder = host.Services.GetService<IProducerBuilder>() ?? throw new InvalidOperationException("Pulsar ProducerBuilder not found");
// var producer = await producerBuilder.BuildProducerAsync<IProducer<byte[]>>("pulsar-producer-1", producerSettings, cts.Token);
//
// var count = 0;
// while(true)
// {
//     var message = new DataSentMessageV1{ConversationId = Guid.NewGuid(), DataId = count, DataName = Random.Shared.GenerateRandomMaleFirstAndLastName(), Type = nameof(DataSentMessageV1)};
//     await producer.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
//     count++;
//     
//     Console.WriteLine($"Produced message to {producerSettings?.Source}: {count}");
//     
//     await Task.Delay(1000);
// }
//
// await producer.DisposeAsync();

// Console.WriteLine($"Finished: {count} messages sent.");



// await host.RunAsync();
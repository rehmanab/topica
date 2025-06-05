using System.Reflection;
using Aws.Topic.Consumer.Host;
using Aws.Topic.Consumer.Host.Settings;
using Aws.Topic.Consumer.Host.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Host.Shared;

Console.WriteLine("******* Starting Aws.Topic.Consumer.Host *******");

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
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        var hostSettings = configuration.GetSection(AwsHostSettings.SectionName).Get<AwsHostSettings>();
        var consumerSettings = configuration.GetSection(AwsConsumerSettings.SectionName).Get<AwsConsumerSettings>();

        if (hostSettings == null) throw new InvalidOperationException($"{nameof(AwsHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (consumerSettings == null) throw new InvalidOperationException($"{nameof(AwsConsumerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new AwsHostSettingsValidator().ValidateAndThrow(hostSettings);
        new AwsConsumerSettingsValidator().ValidateAndThrow(consumerSettings);

        services.AddSingleton(hostSettings);
        services.AddSingleton(consumerSettings);
        
        // Add MessagingPlatform Components
        services.AddAwsTopica(c =>
        {
            c.ProfileName = hostSettings.ProfileName;
            c.AccessKey = hostSettings.AccessKey;
            c.SecretKey = hostSettings.SecretKey;
            c.ServiceUrl = hostSettings.ServiceUrl;
            c.RegionEndpoint = hostSettings.RegionEndpoint;
        }, Assembly.GetAssembly(typeof(ClassToReferenceAssembly)) ?? throw new InvalidOperationException());
        // Assembly.GetExecutingAssembly()
        
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
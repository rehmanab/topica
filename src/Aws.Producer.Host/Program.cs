using System.Reflection;
using Aws.Producer.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Aws.Settings;
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
        
        services.Configure<HostOptions>(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromSeconds(5);
        });
        
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
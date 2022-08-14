using Aws.Consumer.Host;
using Aws.Consumer.Host.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Topica.Settings;

Console.WriteLine("******* Starting Aws.Consumer.Host *******");

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
        // Configuration
        var hostSettings = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        var awsHostSettings = hostSettings.GetSection(AwsHostSettings.SectionName).Get<AwsHostSettings>();
        
        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            return config.GetSection(ConsumerSettings.SectionName).Get<IEnumerable<ConsumerSettings>>();
        });
        
        // Add MessagingPlatform Components
        services.AddAwsTopica(c =>
        {
            c.ProfileName = awsHostSettings.ProfileName;
            c.AccessKey = awsHostSettings.AccessKey;
            c.SecretKey = awsHostSettings.SecretKey;
            c.ServiceUrl = awsHostSettings.ServiceUrl;
            c.RegionEndpoint = awsHostSettings.RegionEndpoint;
        });
        
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
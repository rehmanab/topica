using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.Pulsar;
using Topica.Pulsar.Contracts;
using Xunit;

namespace Topica.Integration.Tests.Pulsar;

public class PulsarTopicSharedFixture : IAsyncLifetime
{
    private PulsarContainer _container = null!;
    public IPulsarTopicCreationBuilder Builder { get; private set; } = null!;
    public static int DelaySeconds => 7;
    
    public async Task InitializeAsync()
    {
        _container = new PulsarBuilder()
            .WithImage("apachepulsar/pulsar:4.0.4")
            .WithName("pulsarbroker-integration-test")
            .WithPortBinding(6651, 6650)
            .WithPortBinding(8081, 8080)
            //.WithEnvironment("PULSAR_MEM", "Xms256m -Xmx256m -XX:MaxDirectMemorySize=256m")
            .Build();
        
        await _container.StartAsync();

        var brokerAddress = _container.GetBrokerAddress();
        var serviceAddress = _container.GetServiceAddress();
        
        Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                // Add MessagingPlatform Components
                services.AddPulsarTopica(c =>
                {
                    c.ServiceUrl = brokerAddress.EndsWith('/') ? brokerAddress.Remove(brokerAddress.LastIndexOf('/')) : brokerAddress;
                    c.PulsarManagerBaseUrl = null; // No Pulsar Manager web frontend in this test
                    c.PulsarAdminBaseUrl = serviceAddress.EndsWith('/') ? serviceAddress.Remove(serviceAddress.LastIndexOf('/')) : serviceAddress;
                }, Assembly.GetExecutingAssembly());

                Builder = services.BuildServiceProvider().GetRequiredService<IPulsarTopicCreationBuilder>();
            }).Build();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();    
    }
}
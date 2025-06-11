using System.Net;
using System.Reflection;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Topica.Azure.ServiceBus.Contracts;
using Topica.Integration.Tests.Shared;
using Xunit;

namespace Topica.Integration.Tests.Azure.ServiceBus;

public class AzureServiceBusTopicSharedFixture : IAsyncLifetime
{
    private INetwork _containersNetwork = null!;
    private IContainer _azureSqlEdgeContainer = null!;
    private IContainer _serviceBusContainer = null!;
    public IAzureServiceBusTopicCreationBuilder Builder { get; private set; } = null!;
    public static int DelaySeconds => 7;

    // Restart (Quit, Start) Docker Desktop on macOS, ensure to delete any networks that are lingering, otherwise the Sql Edge container might not start properly.
    public async Task InitializeAsync()
    {
        var currentDomainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var configFilePath = Path.Join(currentDomainBaseDirectory, "Azure", "ServiceBus", "AzureServiceBusConfig.json");
        const string sqlPassword = "Password123!";
        var sqlContainerName = $"sqledge-integration-tests-{Guid.NewGuid()}";

        _containersNetwork = new NetworkBuilder()
            .WithName(sqlContainerName)
            .Build();

        _azureSqlEdgeContainer = new ContainerBuilder()
            .WithImage("mcr.microsoft.com/azure-sql-edge:latest")
            .WithName(sqlContainerName)
            .WithHostname(sqlContainerName)
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("MSSQL_SA_PASSWORD", sqlPassword)
            .WithNetwork(_containersNetwork)
            .WithNetworkAliases(sqlContainerName)
            // .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("SQL Server is now ready for client connections.", w => w.WithTimeout(TimeSpan.FromSeconds(30))))
            .WithOutputConsumer(Consume.RedirectStdoutAndStderrToStream(Console.OpenStandardOutput(), Console.OpenStandardError()))
            .Build();
        
        _serviceBusContainer = new ContainerBuilder()
            .DependsOn(_azureSqlEdgeContainer)
            .WithImage("mcr.microsoft.com/azure-messaging/servicebus-emulator:latest")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("MSSQL_SA_PASSWORD", sqlPassword)
            .WithEnvironment("SQL_SERVER", sqlContainerName)
            .WithPortBinding(5672, true)
            .WithPortBinding(5300, true)
            .WithBindMount(configFilePath, "/ServiceBus_Emulator/ConfigFiles/Config.json")
            .WithNetwork(_containersNetwork)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(5300).ForPath("/health").ForStatusCode(HttpStatusCode.OK)))
            //.WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Emulator Service is Successfully Up!", w => w.WithTimeout(TimeSpan.FromSeconds(60))))
            //.WithOutputConsumer(Consume.RedirectStdoutAndStderrToStream(Console.OpenStandardOutput(), Console.OpenStandardError()))
            // .WithLogger(logger)
            .Build();

        await _containersNetwork.CreateAsync();
        await _azureSqlEdgeContainer.StartAsync();
        await _serviceBusContainer.StartAsync();

        var connectionString = Utilities.GetConnectionString("127.0.0.1", _serviceBusContainer.GetMappedPublicPort(5672));

        await Task.Delay(TimeSpan.FromSeconds(2));

        Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                // Add MessagingPlatform Components
                services.AddAzureServiceBusTopica(c => { c.ConnectionString = connectionString; }, Assembly.GetExecutingAssembly());

                Builder = services.BuildServiceProvider().GetRequiredService<IAzureServiceBusTopicCreationBuilder>();
            }).Build();
    }

    public async Task DisposeAsync()
    {
        await _azureSqlEdgeContainer.DisposeAsync();
        await _serviceBusContainer.DisposeAsync();
        await _containersNetwork.DeleteAsync();
    }
}
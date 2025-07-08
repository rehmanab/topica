using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.LocalStack;
using Topica.Aws.Contracts;
using Xunit;

namespace Topica.Integration.Tests.Aws.AwsQueue;

// Created once for all tests of the same collection name
// because of Collection(nameof(AwsQueueCollection)) attribute on the test classes

public class AwsQueueSharedFixture : IAsyncLifetime
{
    private LocalStackContainer _container = null!;
    public IAwsQueueCreationBuilder Builder { get; private set; } = null!;
    public static int DelaySeconds => 7;

    public async Task InitializeAsync()
    {
        // Localstack TestContainer
        _container = new LocalStackBuilder()
            .WithName("localstack-aws-queue-integration-test")
            .WithImage("localstack/localstack:4.4.0")
            .Build();

        await _container.StartAsync();
        var serviceUrl = _container.GetConnectionString();

        Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, services) =>
            {
                // Add MessagingPlatform Components
                services.AddAwsTopica(c => { c.ServiceUrl = serviceUrl; }, Assembly.GetExecutingAssembly());

                Builder = services.BuildServiceProvider().GetRequiredService<IAwsQueueCreationBuilder>();
            }).Build();
        
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
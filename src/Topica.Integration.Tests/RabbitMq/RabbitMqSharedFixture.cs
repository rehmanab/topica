using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.RabbitMq;
using Topica.RabbitMq.Contracts;
using Xunit;

namespace Topica.Integration.Tests.RabbitMq;

public class RabbitMqSharedFixture : IAsyncLifetime
{
    private RabbitMqContainer _container = null!;
    public IRabbitMqQueueCreationBuilder QueueBuilder { get; private set; } = null!;
    public IRabbitMqTopicCreationBuilder TopicBuilder { get; private set; } = null!;
    public static int DelaySeconds => 7;

    // Need to restart docker if RabbitMQ container fails to start
    public async Task InitializeAsync()
    {
        _container = new RabbitMqBuilder()
            .WithImage("rabbitmq:4.1.0-management")
            .WithName("rabbitmq-integration-test")
            .WithHostname("rabbitmq-integration-test")
            .WithUsername("guest").WithPassword("guest")
            .WithPortBinding(15672, 15672)
            .WithPortBinding(5672, 5672)
            .WithExposedPort(5672)
            .Build();

        await _container.StartAsync();

        // var connectionString = _container.GetConnectionString();
        var mappedPublicPort = _container.GetMappedPublicPort(5672);

        Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                // Add MessagingPlatform Components
                services.AddRabbitMqTopica(c =>
                {
                    c.Hostname = "127.0.0.1";
                    c.UserName = "guest";
                    c.Password = "guest";
                    c.Scheme = "amqp";
                    c.Port = mappedPublicPort;
                    c.ManagementPort = 15672;
                    c.ManagementScheme = "http";
                    c.VHost = "Test";
                }, Assembly.GetExecutingAssembly());

                QueueBuilder = services.BuildServiceProvider().GetRequiredService<IRabbitMqQueueCreationBuilder>();
                TopicBuilder = services.BuildServiceProvider().GetRequiredService<IRabbitMqTopicCreationBuilder>();
            }).Build();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
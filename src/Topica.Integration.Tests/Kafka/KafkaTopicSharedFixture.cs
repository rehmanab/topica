using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.Kafka;
using Topica.Kafka.Contracts;
using Xunit;

namespace Topica.Integration.Tests.Kafka;

public class KafkaTopicSharedFixture : IAsyncLifetime
{
    private KafkaContainer _container = null!;
    public string BootstrapServerAddress { get; private set; } = null!;
    public IKafkaTopicCreationBuilder Builder { get; private set; } = null!;
    public static int DelaySeconds => 7;

    public async Task InitializeAsync()
    {
        _container = new KafkaBuilder()
            .WithImage("confluentinc/cp-server:7.9.0")
            .WithName("broker-integration-tests")
            .WithHostname("broker-integration-tests")
            .WithPortBinding(9093, 9092)
            .WithPortBinding(9102, 9101)
            //.WithEnvironment("KAFKA_NODE_ID", "1")
            //.WithEnvironment("KAFKA_LISTENER_SECURITY_PROTOCOL_MAP", "CONTROLLER:PLAINTEXT,PLAINTEXT:PLAINTEXT,PLAINTEXT_HOST:PLAINTEXT")
            //.WithEnvironment("KAFKA_ADVERTISED_LISTENERS", "PLAINTEXT://broker:29092,PLAINTEXT_HOST://dockerhost:9092")
            .WithEnvironment("KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR", "1")
            .WithEnvironment("KAFKA_GROUP_INITIAL_REBALANCE_DELAY_MS", "0")
            .WithEnvironment("KAFKA_CONFLUENT_LICENSE_TOPIC_REPLICATION_FACTOR", "1")
            .WithEnvironment("KAFKA_CONFLUENT_BALANCER_TOPIC_REPLICATION_FACTOR", "1")
            .WithEnvironment("KAFKA_TRANSACTION_STATE_LOG_MIN_ISR", "1")
            .WithEnvironment("KAFKA_TRANSACTION_STATE_LOG_REPLICATION_FACTOR", "1")
            //.WithEnvironment("KAFKA_JMX_PORT", "9101")
            //.WithEnvironment("KAFKA_JMX_HOSTNAME", "localhost")
            //.WithEnvironment("KAFKA_CONFLUENT_SCHEMA_REGISTRY_URL", "http://schema-registry:8081")
            //.WithEnvironment("KAFKA_METRIC_REPORTERS", "io.confluent.metrics.reporter.ConfluentMetricsReporter")
            //.WithEnvironment("CONFLUENT_METRICS_REPORTER_BOOTSTRAP_SERVERS", "broker:29092")
            //.WithEnvironment("CONFLUENT_METRICS_REPORTER_TOPIC_REPLICAS", "1")
            //.WithEnvironment("KAFKA_PROCESS_ROLES", "broker,controller")
            //.WithEnvironment("KAFKA_CONTROLLER_QUORUM_VOTERS", "1@broker:29093")
            //.WithEnvironment("KAFKA_LISTENERS", "PLAINTEXT://broker:29092,CONTROLLER://broker:29093,PLAINTEXT_HOST://0.0.0.0:9092")
            //.WithEnvironment("KAFKA_INTER_BROKER_LISTENER_NAME", "PLAINTEXT")
            //.WithEnvironment("KAFKA_CONTROLLER_LISTENER_NAMES", "CONTROLLER")
            //.WithEnvironment("KAFKA_LOG_DIRS", "/tmp/kraft-combined-logs")
            //.WithEnvironment("CONFLUENT_METRICS_ENABLE", "true")
            //.WithEnvironment("CONFLUENT_SUPPORT_CUSTOMER_ID", "anonymous")
            //.WithEnvironment("CLUSTER_ID", "MkU3OEVBNTcwNTJENDM3Qk")
            .Build();
        
        await _container.StartAsync();

        BootstrapServerAddress = _container.GetBootstrapAddress();
        var mappedPublicPort = _container.GetMappedPublicPort(9092);
        
        Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, services) =>
            {
                // Add MessagingPlatform Components
                services.AddKafkaTopica(c =>
                {
                    c.BootstrapServers = [BootstrapServerAddress];
                }, Assembly.GetExecutingAssembly());

                Builder = services.BuildServiceProvider().GetRequiredService<IKafkaTopicCreationBuilder>();
            }).Build();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
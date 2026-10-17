<Query Kind="Program">
  <NuGetReference>Microsoft.Extensions.Logging.Console</NuGetReference>
  <NuGetReference>Topica.Kafka</NuGetReference>
  <Namespace>Confluent.Kafka</Namespace>
  <Namespace>Confluent.Kafka.Admin</Namespace>
  <Namespace>Confluent.Kafka.SyncOverAsync</Namespace>
  <Namespace>Microsoft.DotNet.PlatformAbstractions</Namespace>
  <Namespace>Microsoft.Extensions.Configuration</Namespace>
  <Namespace>Microsoft.Extensions.Configuration.Memory</Namespace>
  <Namespace>Microsoft.Extensions.DependencyInjection</Namespace>
  <Namespace>Microsoft.Extensions.DependencyInjection.Extensions</Namespace>
  <Namespace>Microsoft.Extensions.DependencyModel</Namespace>
  <Namespace>Microsoft.Extensions.DependencyModel.Resolution</Namespace>
  <Namespace>Microsoft.Extensions.Logging</Namespace>
  <Namespace>Microsoft.Extensions.Logging.Abstractions</Namespace>
  <Namespace>Microsoft.Extensions.Logging.Configuration</Namespace>
  <Namespace>Microsoft.Extensions.Logging.Console</Namespace>
  <Namespace>Microsoft.Extensions.Options</Namespace>
  <Namespace>Microsoft.Extensions.Primitives</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Bson</Namespace>
  <Namespace>Newtonsoft.Json.Converters</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>Newtonsoft.Json.Schema</Namespace>
  <Namespace>Newtonsoft.Json.Serialization</Namespace>
  <Namespace>Polly</Namespace>
  <Namespace>Polly.Bulkhead</Namespace>
  <Namespace>Polly.Caching</Namespace>
  <Namespace>Polly.CircuitBreaker</Namespace>
  <Namespace>Polly.Fallback</Namespace>
  <Namespace>Polly.Hedging</Namespace>
  <Namespace>Polly.NoOp</Namespace>
  <Namespace>Polly.RateLimit</Namespace>
  <Namespace>Polly.Registry</Namespace>
  <Namespace>Polly.Retry</Namespace>
  <Namespace>Polly.Simmy</Namespace>
  <Namespace>Polly.Simmy.Behavior</Namespace>
  <Namespace>Polly.Simmy.Fault</Namespace>
  <Namespace>Polly.Simmy.Latency</Namespace>
  <Namespace>Polly.Simmy.Outcomes</Namespace>
  <Namespace>Polly.Telemetry</Namespace>
  <Namespace>Polly.Timeout</Namespace>
  <Namespace>Polly.Utilities</Namespace>
  <Namespace>Polly.Wrap</Namespace>
  <Namespace>Scrutor</Namespace>
  <Namespace>System.Diagnostics.Metrics</Namespace>
  <Namespace>System.IO.Pipelines</Namespace>
  <Namespace>System.Runtime.InteropServices</Namespace>
  <Namespace>System.Text.Encodings.Web</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>System.Text.Json.Schema</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Text.Json.Serialization.Metadata</Namespace>
  <Namespace>System.Text.Unicode</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Topica</Namespace>
  <Namespace>Topica.Contracts</Namespace>
  <Namespace>Topica.Executors</Namespace>
  <Namespace>Topica.Helpers</Namespace>
  <Namespace>Topica.Kafka.Builders</Namespace>
  <Namespace>Topica.Kafka.Consumers</Namespace>
  <Namespace>Topica.Kafka.Contracts</Namespace>
  <Namespace>Topica.Kafka.Producers</Namespace>
  <Namespace>Topica.Kafka.Providers</Namespace>
  <Namespace>Topica.Messages</Namespace>
  <Namespace>Topica.Resolvers</Namespace>
  <Namespace>Topica.Services</Namespace>
  <Namespace>Topica.Settings</Namespace>
  <Namespace>Topica.Topics</Namespace>
</Query>

#load "Projects\Topica\TopicaMessages"

private IProducer _producer1 = null!;
private Microsoft.Extensions.Logging.ILogger _logger = null!;

async Task Main()
{
	var services = new ServiceCollection();

	services.AddLogging(configure =>
		configure
		.AddFilter("*", LogLevel.Debug)
		.AddSimpleConsole(x =>
		{
			x.IncludeScopes = false;
			x.TimestampFormat = "[HH:mm:ss] ";
			x.SingleLine = true;
		}));

	string[] bootstrapServers = ["localhost:9092"];

	// Add MessagingPlatform Components
	services.AddKafkaTopica(c =>
	{
		c.BootstrapServers = bootstrapServers;
	}, Assembly.GetExecutingAssembly());

	var provider = services.BuildServiceProvider();
	var builder = provider.GetService<IKafkaTopicCreationBuilder>()!;
	var loggerFactory = provider.GetService<ILoggerFactory>()!;

	_logger = loggerFactory.CreateLogger("LINQPAD");

	var cts = new CancellationTokenSource();
	var cancellationToken = cts.Token;

	var topicName = "topica_web_analytics_topic_v1";
	_producer1 = await builder
		.WithWorkerName(nameof(CustomEventMessageV1))
		.WithTopicName(topicName)
		.WithConsumerGroup("consumer-group-1")
		.WithTopicSettings(false, 6)
		.WithBootstrapServers(bootstrapServers)
		.BuildProducerAsync(cancellationToken);

	var count = await SendSingleAsync(topicName, cancellationToken);
	// var count = await SendBatchAsync(topicName, cancellationToken);

	await _producer1.DisposeAsync();

	_logger.LogInformation("Finished: {Count} messages sent", count);
}

private async Task<int> SendSingleAsync(string source, CancellationToken stoppingToken)
    {
        var count = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = new CustomEventMessageV1 { EventId = count, EventName = "custom.event.web.v1", ConversationId = Guid.NewGuid(), Type = nameof(CustomEventMessageV1) };
            
            await _producer1.ProduceAsync(source, message, null, stoppingToken);
            _logger.LogInformation("Sent to {Topic}: {Count}", source, count);
            count++;

            await Task.Delay(1000, stoppingToken);
        }

        return count;
    }
    
    private async Task<int> SendBatchAsync(string source, CancellationToken stoppingToken)
    {
	var messages = Enumerable.Range(1, 500)
		.Select(index => new CustomEventMessageV1 { EventId = index, EventName = "custom.event.web.v1", ConversationId = Guid.NewGuid(), Type = nameof(CustomEventMessageV1) })
		.Cast<BaseMessage>()
		.ToList();

	await _producer1.ProduceBatchAsync(source, messages, null, stoppingToken);
	_logger.LogInformation("Sent batch to {Topic}: {Count}", source, messages.Count);

	return messages.Count;
}



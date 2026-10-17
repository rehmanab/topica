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
  <Namespace>Topica.Kafka.Configuration</Namespace>
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

#load "Projects\Topica\TopicaMessagesModels"

async Task Main()
{
	var services = new ServiceCollection();

	services.AddLogging(configure => configure
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

	var cts = new CancellationTokenSource();
	var cancellationToken = cts.Token;

	var consumer1 = await builder
		.WithWorkerName(nameof(CustomEventMessageV1))
		.WithTopicName("topica_web_analytics_topic_v1")
		.WithConsumerGroup("consumer-group-1")
		.WithTopicSettings(false, 6)
		.WithBootstrapServers(bootstrapServers)
		.WithConsumeSettings(1)
		.BuildConsumerAsync(cancellationToken);
		
	await consumer1.ConsumeAsync(cancellationToken);
}

public class CustomEventMessageV1HandlerV1(ILogger<CustomEventMessageV1HandlerV1> logger) : IHandler<CustomEventMessageV1>
{
	public async Task<bool> HandleAsync(CustomEventMessageV1 source, Dictionary<string, string>? properties)
	{
		logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Event: {Event}", nameof(CustomEventMessageV1), source.ConversationId, $"{source.EventId} : {source.EventName}");

		return await Task.FromResult(true);
	}

	/// <summary>
	/// Validate the message type equals the message name and can validate any properties for the message
	/// </summary>
	/// <param name="message"></param>
	/// <returns>True if Valid</returns>
	public bool ValidateMessage(CustomEventMessageV1 message)
	{
		if (string.IsNullOrWhiteSpace(message.Type) || !message.Type.Equals(nameof(CustomEventMessageV1), StringComparison.CurrentCultureIgnoreCase))
		{
			logger.LogWarning("Invalid message type: {MessageType} for Handler: {Handler}", message.Type, nameof(CustomEventMessageV1));
			return false;
		}

		return true;
	}
}



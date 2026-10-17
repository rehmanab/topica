<Query Kind="Program">
  <NuGetReference>Microsoft.Extensions.Diagnostics</NuGetReference>
  <NuGetReference>Microsoft.Extensions.Hosting</NuGetReference>
  <NuGetReference>Microsoft.Extensions.Hosting.Abstractions</NuGetReference>
  <NuGetReference>Microsoft.Extensions.Logging.Console</NuGetReference>
  <NuGetReference Version="1.1.3">Topica.Pulsar</NuGetReference>
  <Namespace>Microsoft.Extensions.Configuration</Namespace>
  <Namespace>Microsoft.Extensions.Configuration.Memory</Namespace>
  <Namespace>Microsoft.Extensions.DependencyInjection</Namespace>
  <Namespace>Microsoft.Extensions.DependencyInjection.Extensions</Namespace>
  <Namespace>Microsoft.Extensions.Diagnostics.Metrics</Namespace>
  <Namespace>Microsoft.Extensions.Diagnostics.Metrics.Configuration</Namespace>
  <Namespace>Microsoft.Extensions.Internal</Namespace>
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
  <Namespace>System.Diagnostics.Eventing.Reader</Namespace>
  <Namespace>System.Diagnostics.Metrics</Namespace>
  <Namespace>System.IO.Pipelines</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Runtime.InteropServices</Namespace>
  <Namespace>System.Runtime.Serialization.Formatters</Namespace>
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
  <Namespace>Topica.Messages</Namespace>
  <Namespace>Topica.Pulsar.Contracts</Namespace>
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

	// Add MessagingPlatform Components
	services.AddPulsarTopica(c =>
		{
			c.ServiceUrl = "pulsar://localhost:6650";
			c.PulsarManagerBaseUrl = "http://localhost:7750";
			c.PulsarAdminBaseUrl = "http://localhost:8080";
		}, Assembly.GetExecutingAssembly());

	var provider = services.BuildServiceProvider();
	var builder = provider.GetService<IPulsarTopicCreationBuilder>()!;

	var cts = new CancellationTokenSource();
	var cancellationToken = cts.Token;

	var consumer1 = await builder
		.WithWorkerName(nameof(FileDownloadedMessageV1))
		.WithTopicName("topica_web_analytics_topic_v1")
		.WithConsumerGroup("analytics_group")
		.WithConfiguration("public", "topica", 6)
		.WithTopicOptions(false)
		.WithConsumeSettings(1)
		.BuildConsumerAsync(cancellationToken);
	
	await consumer1.ConsumeAsync(cancellationToken);
}

public class FileDownloadedMessageHandlerV1(ILogger<FileDownloadedMessageHandlerV1> logger) : IHandler<FileDownloadedMessageV1>
{
	public async Task<bool> HandleAsync(FileDownloadedMessageV1 source, Dictionary<string, string>? properties)
	{
		logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Event: {Event}", nameof(FileDownloadedMessageV1), source.ConversationId, $"{source.EventId} : {source.EventName}");

		return await Task.FromResult(true);
	}

	/// <summary>
	/// Validate the message type equals the message name and can validate any properties for the message
	/// </summary>
	/// <param name="message"></param>
	/// <returns>True if Valid</returns>
	public bool ValidateMessage(FileDownloadedMessageV1 message)
	{
		if (string.IsNullOrWhiteSpace(message.Type) || !message.Type.Equals(nameof(FileDownloadedMessageV1), StringComparison.CurrentCultureIgnoreCase))
		{
			logger.LogWarning("Invalid message type: {MessageType} for Handler: {Handler}", message.Type, nameof(FileDownloadedMessageV1));
			return false;
		}

		return true;
	}
}


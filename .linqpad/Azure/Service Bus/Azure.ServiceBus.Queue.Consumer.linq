<Query Kind="Program">
  <NuGetReference>Microsoft.Extensions.Logging.Console</NuGetReference>
  <NuGetReference Version="1.1.3">Topica.Azure.ServiceBus</NuGetReference>
  <Namespace>Azure</Namespace>
  <Namespace>Azure.Core</Namespace>
  <Namespace>Azure.Core.Amqp</Namespace>
  <Namespace>Azure.Core.Cryptography</Namespace>
  <Namespace>Azure.Core.Diagnostics</Namespace>
  <Namespace>Azure.Core.Extensions</Namespace>
  <Namespace>Azure.Core.GeoJson</Namespace>
  <Namespace>Azure.Core.Pipeline</Namespace>
  <Namespace>Azure.Core.Serialization</Namespace>
  <Namespace>Azure.Messaging</Namespace>
  <Namespace>Azure.Messaging.ServiceBus</Namespace>
  <Namespace>Azure.Messaging.ServiceBus.Administration</Namespace>
  <Namespace>Microsoft.Azure.Amqp</Namespace>
  <Namespace>Microsoft.Azure.Amqp.Encoding</Namespace>
  <Namespace>Microsoft.Azure.Amqp.Framing</Namespace>
  <Namespace>Microsoft.Azure.Amqp.Sasl</Namespace>
  <Namespace>Microsoft.Azure.Amqp.Serialization</Namespace>
  <Namespace>Microsoft.Azure.Amqp.Transaction</Namespace>
  <Namespace>Microsoft.Azure.Amqp.Transport</Namespace>
  <Namespace>Microsoft.Azure.Amqp.X509</Namespace>
  <Namespace>Microsoft.DotNet.PlatformAbstractions</Namespace>
  <Namespace>Microsoft.Extensions.Azure</Namespace>
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
  <Namespace>System.ClientModel</Namespace>
  <Namespace>System.ClientModel.Primitives</Namespace>
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
  <Namespace>Topica.Azure.ServiceBus.Builders</Namespace>
  <Namespace>Topica.Azure.ServiceBus.Configuration</Namespace>
  <Namespace>Topica.Azure.ServiceBus.Consumers</Namespace>
  <Namespace>Topica.Azure.ServiceBus.Contracts</Namespace>
  <Namespace>Topica.Azure.ServiceBus.Producers</Namespace>
  <Namespace>Topica.Azure.ServiceBus.Providers</Namespace>
  <Namespace>Topica.Contracts</Namespace>
  <Namespace>Topica.Executors</Namespace>
  <Namespace>Topica.Helpers</Namespace>
  <Namespace>Topica.Messages</Namespace>
  <Namespace>Topica.Resolvers</Namespace>
  <Namespace>Topica.Services</Namespace>
  <Namespace>Topica.Settings</Namespace>
  <Namespace>Topica.Topics</Namespace>
  <IncludeAspNet>true</IncludeAspNet>
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
	services.AddAzureServiceBusTopica(c =>
	{
		c.ConnectionString = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";
	}, Assembly.GetExecutingAssembly());

	var provider = services.BuildServiceProvider();
	var builder = provider.GetService<IAzureServiceBusQueueCreationBuilder>()!;

	var cts = new CancellationTokenSource();
	var cancellationToken = cts.Token;

	var consumer1 = await builder
		.WithWorkerName(nameof(VideoPlayedMessageV1))
		.WithQueueName("topica_web_analytics_adhoc_queue1_v6")
		.WithNumberOfInstances(1)
		.BuildConsumerAsync(cancellationToken);

	await consumer1.ConsumeAsync(cancellationToken);
}

public class VideoPlayedMessageHandlerV1(ILogger<VideoPlayedMessageHandlerV1> logger) : IHandler<VideoPlayedMessageV1>
{
	public async Task<bool> HandleAsync(VideoPlayedMessageV1 source, Dictionary<string, string>? properties)
	{
		logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Event: {Event}", nameof(VideoPlayedMessageV1), source.ConversationId, $"{source.EventId} : {source.EventName}");

		return await Task.FromResult(true);
	}

	public bool ValidateMessage(VideoPlayedMessageV1 message)
	{
		if (string.IsNullOrWhiteSpace(message.Type) || !message.Type.Equals(nameof(VideoPlayedMessageV1), StringComparison.CurrentCultureIgnoreCase))
		{
			logger.LogWarning("Invalid message type: {MessageType} for Handler: {Handler}", message.Type, nameof(VideoPlayedMessageV1));
			return false;
		}

		return true;
	}
}


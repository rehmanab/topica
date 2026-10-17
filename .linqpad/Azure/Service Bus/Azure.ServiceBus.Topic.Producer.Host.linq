<Query Kind="Program">
  <NuGetReference>Microsoft.Extensions.Logging.Console</NuGetReference>
  <NuGetReference>Topica.Azure.ServiceBus</NuGetReference>
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
  <Namespace>Microsoft.Extensions.DependencyInjection</Namespace>
  <Namespace>Microsoft.Extensions.DependencyInjection.Extensions</Namespace>
  <Namespace>Microsoft.Extensions.DependencyModel</Namespace>
  <Namespace>Microsoft.Extensions.DependencyModel.Resolution</Namespace>
  <Namespace>Microsoft.Extensions.Logging</Namespace>
  <Namespace>Microsoft.Extensions.Logging.Abstractions</Namespace>
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
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Topica</Namespace>
  <Namespace>Topica.Azure.ServiceBus.Builders</Namespace>
  <Namespace>Topica.Azure.ServiceBus.Configuration</Namespace>
  <Namespace>Topica.Azure.ServiceBus.Consumers</Namespace>
  <Namespace>Topica.Azure.ServiceBus.Contracts</Namespace>
  <Namespace>Topica.Azure.ServiceBus.Providers</Namespace>
  <Namespace>Topica.Contracts</Namespace>
  <Namespace>Topica.Executors</Namespace>
  <Namespace>Topica.Helpers</Namespace>
  <Namespace>Topica.Messages</Namespace>
  <Namespace>Topica.Resolvers</Namespace>
  <Namespace>Topica.Services</Namespace>
  <Namespace>Topica.Settings</Namespace>
  <Namespace>Topica.Topics</Namespace>
  <RemoveNamespace>System.Collections</RemoveNamespace>
  <RemoveNamespace>System.Data</RemoveNamespace>
  <RemoveNamespace>System.Diagnostics</RemoveNamespace>
  <RemoveNamespace>System.IO</RemoveNamespace>
  <RemoveNamespace>System.Linq.Expressions</RemoveNamespace>
  <RemoveNamespace>System.Text</RemoveNamespace>
  <RemoveNamespace>System.Text.RegularExpressions</RemoveNamespace>
  <RemoveNamespace>System.Transactions</RemoveNamespace>
  <RemoveNamespace>System.Xml</RemoveNamespace>
  <RemoveNamespace>System.Xml.Linq</RemoveNamespace>
  <RemoveNamespace>System.Xml.XPath</RemoveNamespace>
  <IncludeAspNet>true</IncludeAspNet>
</Query>

#load "Projects\Topica\TopicaMessagesModels"

private IProducer _producer1 = null!;
private Microsoft.Extensions.Logging.ILogger _logger = null!;

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
	var builder = provider.GetService<IAzureServiceBusTopicCreationBuilder>()!;
	var loggerFactory = provider.GetService<ILoggerFactory>()!;
	
	_logger = loggerFactory.CreateLogger("LINQPAD");

	var cts = new CancellationTokenSource();
	var cancellationToken = cts.Token;

	var topicName = "topica_web_analytics_topic_v1";
	_producer1 = await builder
		.WithWorkerName(nameof(VideoPlayedMessageV1))
		.WithTopicName(topicName)
		.WithSubscriptions([
				new AzureServiceBusTopicSubscriptionSettings { Source = "topica_web_analytics_subscription_sales_v1" },
				new AzureServiceBusTopicSubscriptionSettings { Source = "topica_web_analytics_subscription_reporting_v1" },
		])
		.WithSubscribeToSubscription("topica_web_analytics_subscription_sales_v1")
		.BuildProducerAsync(cancellationToken);

	var count = await SendSingleAsync(topicName, cancellationToken);
	// var count = await SendBatchAsync(topicName, cancellationToken);

	_logger.LogInformation("Finished: {Count} messages sent", count);
}

private async Task<int> SendSingleAsync(string topicName, CancellationToken stoppingToken)
{
	var count = 1;
	while (!stoppingToken.IsCancellationRequested)
	{
		var messageGroupId = Guid.NewGuid().ToString();

		Topica.Messages.BaseMessage message = new VideoPlayedMessageV1
		{
			ConversationId = Guid.NewGuid(),
			EventId = count,
			EventName = "video.played.web.v1",
			Type = nameof(VideoPlayedMessageV1),
			MessageGroupId = messageGroupId
		};

		var messageAttributes = new Dictionary<string, string>
			{
				{"SignatureVersion", "2" }
			};

		await _producer1.ProduceAsync(message, messageAttributes, stoppingToken);

		_logger.LogInformation("Produced single message to {MessagingSettingsSource}: {MessageIdName}", topicName, $"{message.EventId} : {message.EventName}");

		count++;

		await Task.Delay(1000, stoppingToken);
	}

	return count;
}

private async Task<int> SendBatchAsync(string topicName, CancellationToken stoppingToken)
{
	var messageGroupId = Guid.NewGuid();

	var messages = Enumerable.Range(1, 500)
		.Select(index => new VideoPlayedMessageV1
		{
			ConversationId = Guid.NewGuid(),
			EventId = index,
			EventName = "video.played.web.v1",
			Type = nameof(VideoPlayedMessageV1),
			MessageGroupId = messageGroupId.ToString()
		})
		.Cast<BaseMessage>()
		.ToList();

	var messageAttributes = new Dictionary<string, string>
		{
			{"SignatureVersion", "2" }
		};

	await _producer1.ProduceBatchAsync(messages, messageAttributes, stoppingToken);

	_logger.LogInformation("Produced ({Count}) batch messages in groups of 10 for AWS to {MessagingSettingsSource}", messages.Count, topicName);

	return messages.Count;
}


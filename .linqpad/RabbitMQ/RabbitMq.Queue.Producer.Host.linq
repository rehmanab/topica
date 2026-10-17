<Query Kind="Program">
  <NuGetReference>Microsoft.Extensions.Diagnostics</NuGetReference>
  <NuGetReference>Microsoft.Extensions.Hosting</NuGetReference>
  <NuGetReference>Microsoft.Extensions.Hosting.Abstractions</NuGetReference>
  <NuGetReference>Microsoft.Extensions.Logging.Console</NuGetReference>
  <NuGetReference>Topica.RabbitMq</NuGetReference>
  <Namespace>Microsoft.DotNet.PlatformAbstractions</Namespace>
  <Namespace>Microsoft.Extensions.Configuration</Namespace>
  <Namespace>Microsoft.Extensions.Configuration.Memory</Namespace>
  <Namespace>Microsoft.Extensions.DependencyInjection</Namespace>
  <Namespace>Microsoft.Extensions.DependencyInjection.Extensions</Namespace>
  <Namespace>Microsoft.Extensions.DependencyModel</Namespace>
  <Namespace>Microsoft.Extensions.DependencyModel.Resolution</Namespace>
  <Namespace>Microsoft.Extensions.Diagnostics.Metrics</Namespace>
  <Namespace>Microsoft.Extensions.Diagnostics.Metrics.Configuration</Namespace>
  <Namespace>Microsoft.Extensions.Http</Namespace>
  <Namespace>Microsoft.Extensions.Http.Logging</Namespace>
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
  <Namespace>RabbitMQ.Client</Namespace>
  <Namespace>RabbitMQ.Client.Events</Namespace>
  <Namespace>RabbitMQ.Client.Exceptions</Namespace>
  <Namespace>RabbitMQ.Client.Logging</Namespace>
  <Namespace>Scrutor</Namespace>
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
  <Namespace>System.Threading.RateLimiting</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Topica</Namespace>
  <Namespace>Topica.Contracts</Namespace>
  <Namespace>Topica.Executors</Namespace>
  <Namespace>Topica.Helpers</Namespace>
  <Namespace>Topica.Messages</Namespace>
  <Namespace>Topica.RabbitMq.Builders</Namespace>
  <Namespace>Topica.RabbitMq.Clients</Namespace>
  <Namespace>Topica.RabbitMq.Configuration</Namespace>
  <Namespace>Topica.RabbitMq.Consumers</Namespace>
  <Namespace>Topica.RabbitMq.Contracts</Namespace>
  <Namespace>Topica.RabbitMq.Models</Namespace>
  <Namespace>Topica.RabbitMq.Producers</Namespace>
  <Namespace>Topica.RabbitMq.Providers</Namespace>
  <Namespace>Topica.RabbitMq.Requests</Namespace>
  <Namespace>Topica.Resolvers</Namespace>
  <Namespace>Topica.Services</Namespace>
  <Namespace>Topica.Settings</Namespace>
  <Namespace>Topica.Topics</Namespace>
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
	services.AddRabbitMqTopica(c =>
		{
			c.Hostname = "localhost";
			c.UserName = "guest";
			c.Password = "guest";
			c.Scheme = "amqp";
			c.Port = 5673;
			c.ManagementPort = 15672;
			c.ManagementScheme = "http";
			c.VHost = "topica";
		}, Assembly.GetExecutingAssembly());

	var provider = services.BuildServiceProvider();
	var builder = provider.GetService<IRabbitMqQueueCreationBuilder>()!;
	var loggerFactory = provider.GetService<ILoggerFactory>()!;

	_logger = loggerFactory.CreateLogger("LINQPAD");

	var cts = new CancellationTokenSource();
	var cancellationToken = cts.Token;

	var queueName = "topica_web_analytics_adhoc_queue1_v1";
	_producer1 = await builder
		.WithWorkerName(nameof(SearchTriggeredMessageV1))
		.WithQueueName(queueName)
		.BuildProducerAsync(cancellationToken);

	var count = await SendSingleAsync(queueName, cancellationToken);
	// var count = await SendBatchAsync(queueName, cancellationToken);

	_logger.LogInformation("Finished: {Count} messages sent", count);
}

private async Task<int> SendSingleAsync(string topicName, CancellationToken stoppingToken)
    {
        var count = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = new SearchTriggeredMessageV1 { ConversationId = Guid.NewGuid(), EventId = count, EventName = "search.triggered.web.v1", Type = nameof(SearchTriggeredMessageV1) };

            await _producer1.ProduceAsync(message, null, stoppingToken);

            _logger.LogInformation("Produced message to {MessagingSettingsSource}: {MessageIdName}", topicName, $"{message.EventId} : {message.EventName}");
            count++;
            
            await Task.Delay(1000, stoppingToken);
        }

        return count;
    }
    
    private async Task<int> SendBatchAsync(string topicName, CancellationToken stoppingToken)
    {
        var messages = Enumerable.Range(1, 500)
            .Select(index => new SearchTriggeredMessageV1
			{
				ConversationId = Guid.NewGuid(),
				EventId = index,
				EventName = "search.triggered.web.v1",
				Type = nameof(SearchTriggeredMessageV1)
			})
			.Cast<BaseMessage>()
			.ToList();

	await _producer1.ProduceBatchAsync(messages, null, stoppingToken);

	_logger.LogInformation("Produced ({Count}) batch messages in groups of 10 for AWS to {MessagingSettingsSource}", messages.Count, topicName);

	return messages.Count;
}


<Query Kind="Program">
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <NuGetReference>Topica</NuGetReference>
  <Namespace>Microsoft.Extensions.DependencyInjection</Namespace>
  <Namespace>Microsoft.Extensions.DependencyInjection.Extensions</Namespace>
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
  <Namespace>Topica</Namespace>
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
  <RemoveNamespace>System.Reflection</RemoveNamespace>
  <RemoveNamespace>System.Text</RemoveNamespace>
  <RemoveNamespace>System.Text.RegularExpressions</RemoveNamespace>
  <RemoveNamespace>System.Threading</RemoveNamespace>
  <RemoveNamespace>System.Transactions</RemoveNamespace>
  <RemoveNamespace>System.Xml</RemoveNamespace>
  <RemoveNamespace>System.Xml.Linq</RemoveNamespace>
  <RemoveNamespace>System.Xml.XPath</RemoveNamespace>
</Query>

public class AwsNotification
{
	public string Type { get; set; }
	public string MessageId { get; set; }
	public string TopicArn { get; set; }
	public string Message { get; set; }
	public string Timestamp { get; set; }
	public string UnsubscribeURL { get; set; }
	public MessageAttributes MessageAttributes { get; set; }
	public string SequenceNumber { get; set; }

	public static AwsNotification Parse(string messageBody)
	{
		return JsonConvert.DeserializeObject<AwsNotification>(messageBody);
	}
}

public class MessageAttributes
{
	public SignatureVersion SignatureVersion { get; set; }
}

public class SignatureVersion
{
	public string Type { get; set; }
	public string Value { get; set; }
}

// Messages

public class ButtonClickedMessageV1 : BaseMessage
{
	public string? ButtonText { get; set; }
	public string? ButtonId { get; set; }
	public string? UserId { get; set; }
	public string? SessionId { get; set; }
}

public class CookiesAcceptedMessageV1 : BaseMessage
{
	public string? UserId { get; set; }
	public string? SessionId { get; set; }
	public bool IsAccepted { get; set; } = false;
}

public class CustomEventMessageV1 : BaseMessage
{
	public string? UserId { get; set; }
	public string? SessionId { get; set; }
	public Dictionary<string, string>? EventData { get; set; } = new();
}

public class FileDownloadedMessageV1 : BaseMessage
{
	public string? FileName { get; set; }
	public string? FileUrl { get; set; }
	public string? UserId { get; set; }
	public string? SessionId { get; set; }
}

public class LinkClickedMessageV1 : BaseMessage
{
	public string? LinkText { get; set; }
	public string? LinkUrl { get; set; }
	public string? UserId { get; set; }
	public string? SessionId { get; set; }
}

public class PageLoadedMessageV1 : BaseMessage
{
	public string? PageName { get; set; }
	public string? PageUrl { get; set; }
	public string? UserId { get; set; }
	public string? SessionId { get; set; }
}

public class SearchTriggeredMessageV1 : BaseMessage
{
	public string? SearchTerm { get; set; }
	public string? SearchType { get; set; }
}

public class UserLoginMessageV1 : BaseMessage
{
	public string? UserId { get; set; }
	public string? SessionId { get; set; }
}

public class VideoPlayedMessageV1 : BaseMessage
{
	public string? VideoId { get; set; }
	public string? UserId { get; set; }
	public string? SessionId { get; set; }
	public TimeSpan? VideoPosition { get; set; }
}
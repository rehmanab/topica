using Topica.Messages;

namespace Topica.Integration.Tests.Shared;

public static class MessageCounter
{
    public static List<MessageAttributePair> AwsQueueMessageSent { get; set; } = [];
    public static List<MessageAttributePair> AwsQueueMessageReceived { get; set; } = [];
    
    public static List<MessageAttributePair> AwsTopicMessageSent { get; set; } = [];
    public static List<MessageAttributePair> AwsTopicMessageReceived { get; set; } = [];
    
    public static List<MessageAttributePair> AzureServiceBusQueueMessageSent { get; set; } = [];
    public static List<MessageAttributePair> AzureServiceBusQueueMessageReceived { get; set; } = [];
    
    public static List<MessageAttributePair> AzureServiceBusTopicMessageSent { get; set; } = [];
    public static List<MessageAttributePair> AzureServiceBusTopicMessageReceived { get; set; } = [];
    
    public static List<MessageAttributePair> KafkaTopicMessageSent { get; set; } = [];
    public static List<MessageAttributePair> KafkaTopicMessageReceived { get; set; } = [];
    
    public static List<MessageAttributePair> PulsarTopicMessageSent { get; set; } = [];
    public static List<MessageAttributePair> PulsarTopicMessageReceived { get; set; } = [];
    
    public static List<MessageAttributePair> RabbitMqQueueMessageSent { get; set; } = [];
    public static List<MessageAttributePair> RabbitMqQueueMessageReceived { get; set; } = [];
    
    public static List<MessageAttributePair> RabbitMqTopicMessageSent { get; set; } = [];
    public static List<MessageAttributePair> RabbitMqTopicMessageReceived { get; set; } = [];
}

public class MessageAttributePair
{
    public BaseMessage BaseMessage { get; set; } = null!;
    public Dictionary<string, string>? Attributes { get; set; }
}
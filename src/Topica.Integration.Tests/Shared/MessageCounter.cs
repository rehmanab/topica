using Topica.Messages;

namespace Topica.Integration.Tests.Shared;

public static class MessageCounter
{
    public static List<BaseMessage> AwsQueueMessageSent { get; set; } = [];
    public static List<BaseMessage> AwsQueueMessageReceived { get; set; } = [];
    
    public static List<BaseMessage> AwsTopicMessageSent { get; set; } = [];
    public static List<BaseMessage> AwsTopicMessageReceived { get; set; } = [];
    
    public static List<BaseMessage> AzureServiceBusTopicMessageSent { get; set; } = [];
    public static List<BaseMessage> AzureServiceBusTopicMessageReceived { get; set; } = [];
    
    public static List<BaseMessage> KafkaTopicMessageSent { get; set; } = [];
    public static List<BaseMessage> KafkaTopicMessageReceived { get; set; } = [];
    
    public static List<BaseMessage> PulsarTopicMessageSent { get; set; } = [];
    public static List<BaseMessage> PulsarTopicMessageReceived { get; set; } = [];
    
    public static List<BaseMessage> RabbitMqQueueMessageSent { get; set; } = [];
    public static List<BaseMessage> RabbitMqQueueMessageReceived { get; set; } = [];
    
    public static List<BaseMessage> RabbitMqTopicMessageSent { get; set; } = [];
    public static List<BaseMessage> RabbitMqTopicMessageReceived { get; set; } = [];
}
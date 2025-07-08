namespace Topica.Settings;

public class MessagingSettings
{
    public static string SectionName => nameof(MessagingSettings);

    public string WorkerName { get; set; } = null!;
    public string Source { get; set; } = null!;
    public string SubscribeToSource { get; set; } = null!;
    public int NumberOfInstances { get; set; }

        
    // Aws
    public string[] AwsWithSubscribedQueues { get; set; } = null!;
    public bool AwsIsFifoQueue { get; set; }
    public bool AwsIsFifoContentBasedDeduplication { get; set; }
    public bool AwsBuildWithErrorQueue { get; set; }
    public int AwsErrorQueueMaxReceiveCount { get; set; }
    public int AwsMessageVisibilityTimeoutSeconds { get; set; }
    public int AwsQueueMessageDelaySeconds { get; set; }
    public int AwsQueueMessageRetentionPeriodSeconds { get; set; }
    public int AwsQueueReceiveMessageWaitTimeSeconds { get; set; }
    public int AwsQueueMaximumMessageSizeKb { get; set; }
    public int AwsQueueReceiveMaximumNumberOfMessages { get; set; }
    
    // Azure ServiceBus
    public AzureServiceBusTopicSubscriptionSettings[] AzureServiceBusSubscriptions { get; set; } = null!;
    public string? AzureServiceBusAutoDeleteOnIdle { get; set; }
    public string? AzureServiceBusDefaultMessageTimeToLive { get; set; }
    public string? AzureServiceBusDuplicateDetectionHistoryTimeWindow { get; set; }
    public bool? AzureServiceBusEnableBatchedOperations { get; set; }
    public bool? AzureServiceBusEnablePartitioning { get; set; }
    public int? AzureServiceBusMaxSizeInMegabytes { get; set; }
    public bool? AzureServiceBusRequiresDuplicateDetection { get; set; }
    public string? AzureServiceBusUserMetadata { get; set; }
    public int? AzureServiceBusMaxMessageSizeInKilobytes { get; set; }
    public bool? AzureServiceBusEnabledStatus { get; set; }
    public bool? AzureServiceBusSupportOrdering { get; set; }


    // Kafka
    public string KafkaConsumerGroup { get; set; } = null!;
    public bool KafkaStartFromEarliestMessages { get; set; }
    public int KafkaNumberOfTopicPartitions { get; set; }
    public string[] KafkaBootstrapServers { get; set; } = null!;


    // Pulsar
    public string PulsarTenant { get; set; } = null!;
    public string PulsarNamespace { get; set; } = null!;
    // Each unique name will start cursor at earliest or latest, then read from that position
    // will read all un-acknowledge per consumer group (actually pulsar uses consumer name).
    // i.e. each consumer name is an independent subscription and acknowledges messages per subscription
    public string PulsarConsumerGroup { get; set; } = null!;
    // Sets any NEW consumers only to the earliest cursor position (can't be changed for existing subscription)
    public bool PulsarStartNewConsumerEarliest { get; set; }
    
    // Pulsar Producer settings
    public bool PulsarBlockIfQueueFull { get; set; }
    public int PulsarMaxPendingMessages { get; set; }
    public int PulsarMaxPendingMessagesAcrossPartitions { get; set; }
    public bool PulsarEnableBatching { get; set; }
    public bool PulsarEnableChunking { get; set; }
    public int PulsarBatchingMaxMessages { get; set; }
    public long PulsarBatchingMaxPublishDelayMilliseconds { get; set; }
    public int PulsarTopicNumberOfPartitions { get; set; }

        
    // RabbitMq
    public string[] RabbitMqWithSubscribedQueues { get; set; } = null!;
}
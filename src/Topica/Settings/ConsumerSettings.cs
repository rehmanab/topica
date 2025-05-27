#nullable enable
namespace Topica.Settings
{
    public class ConsumerSettings
    {
        public static string SectionName => nameof(ConsumerSettings);

        public string? Source { get; set; }
        public string? SubscribeToSource { get; set; }
        public int NumberOfInstances { get; set; } = 1;

        
        // Aws
        public string[]? AwsWithSubscribedQueues { get; set; }
        public bool AwsBuildWithErrorQueue { get; set; }
        public int? AwsErrorQueueMaxReceiveCount { get; set; }
        public int? AwsVisibilityTimeout { get; set; }
        public bool AwsIsFifoQueue { get; set; }
        public bool AwsIsFifoContentBasedDeduplication { get; set; }
        public int? AwsMaximumMessageSize { get; set; }
        public int AwsReceiveMaximumNumberOfMessages { get; set; } = 10;
        public int? AwsMessageRetentionPeriod { get; set; }
        public int? AwsDelaySeconds { get; set; }
        public int? AwsReceiveMessageWaitTimeSeconds { get; set; }

        
        // Kafka
        public string? KafkaConsumerGroup { get; set; }
        public bool? KafkaStartFromEarliestMessages { get; set; }
        public int? KafkaNumberOfTopicPartitions { get; set; }
        public string[]? KafkaBootstrapServers { get; set; }

        
        // Pulsar
        public string PulsarTenant { get; set; } = null!;
        public string PulsarNamespace { get; set; } = null!;
        // Each unique name will start cursor at earliest or latest, then read from that position
        // will read all un-acknowledge per consumer group (actually pulsar uses consumer name).
        // i.e. each consumer name is an independent subscription and acknowledges messages per subscription
        public string PulsarConsumerGroup { get; set; } = null!;
        // Sets any NEW consumers only to the earliest cursor position (can't be changed for existing subscription)
        public bool? PulsarStartNewConsumerEarliest { get; set; }

        
        // RabbitMq
        public string[]? RabbitMqWithSubscribedQueues { get; set; }

        
        // Azure ServiceBus
        public AzureServiceBusTopicSubscriptionSettings[]? AzureServiceBusSubscriptions { get; set; }
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
    }
}
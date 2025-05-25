namespace Topica.Settings
{
    public class ProducerSettings
    {
        public static string SectionName => nameof(ProducerSettings);

        public string Source { get; set; }
        public string[] WithSubscribedQueues { get; set; }

        // Aws
        public bool AwsBuildWithErrorQueue { get; set; }
        public int? AwsErrorQueueMaxReceiveCount { get; set; }
        public int? AwsVisibilityTimeout { get; set; }
        public bool AwsIsFifoQueue { get; set; }
        public bool AwsIsFifoContentBasedDeduplication { get; set; }
        public int? AwsMaximumMessageSize { get; set; }
        public int? AwsMaximumNumberOfMessages { get; set; }
        public int? AwsMessageRetentionPeriod { get; set; }
        public int? AwsDelaySeconds { get; set; }
        public int? AwsReceiveMessageWaitTimeSeconds { get; set; }
        
        // Kafka
        public string KafkaConsumerGroup { get; set; }
        public bool KafkaStartFromEarliestMessages { get; set; }
        public int KafkaNumberOfTopicPartitions { get; set; }
        public string[] KafkaBootstrapServers { get; set; }
        
        // Pulsar
        public string PulsarTenant { get; set; }
        public string PulsarNamespace { get; set; }
        public bool PulsarBlockIfQueueFull { get; set; }
        public int PulsarMaxPendingMessages { get; set; }
        public int PulsarMaxPendingMessagesAcrossPartitions { get; set; }
        public bool PulsarEnableBatching { get; set; }
        public bool PulsarEnableChunking { get; set; }
        public int PulsarBatchingMaxMessages { get; set; }
        public long PulsarBatchingMaxPublishDelayMilliseconds { get; set; }
        
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
        public string ConnectionString { get; set; }
    }
}
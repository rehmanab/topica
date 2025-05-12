namespace Topica.Settings
{
    public class ConsumerSettings
    {
        public static string SectionName => nameof(ConsumerSettings);
        
        public string MessageToHandle{ get; set; }
        public string Source{ get; set; }
        public string SubscribeToSource{ get; set; }
        public string[] WithSubscribedQueues { get; set; }
        public int NumberOfInstances { get; set; } = 1;
        
        // Aws
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
        public string KafkaConsumerGroup { get; set; }
        public bool KafkaStartFromEarliestMessages { get; set; }
        public int KafkaNumberOfTopicPartitions { get; set; }
        public string[] KafkaBootstrapServers { get; set; }
        
        // Pulsar
        public string PulsarTenant { get; set; }
        public string PulsarNamespace { get; set; }
        // Each unique name will start cursor at earliest or latest, then read from that position
        // will read all un-acknowledge per consumer group (actually pulsar uses consumer name).
        // i.e. each consumer name is an independent subscription and acknowledges messages per subscription
        public string PulsarConsumerGroup { get; set; }
        // Sets any NEW consumers only to earliest cursor position (can't be changed for existing subscription)
        public bool PulsarStartNewConsumerEarliest { get; set; }
    }
}
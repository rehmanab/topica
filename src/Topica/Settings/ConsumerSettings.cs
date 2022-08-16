namespace Topica.Settings
{
    public class ConsumerSettings
    {
        public static string SectionName => nameof(ConsumerSettings);
        
        public string MessageToHandle{ get; set; }
        public string Source{ get; set; }
        public string SubscribeToSource{ get; set; }
        public string[] WithSubscribedQueues { get; set; }
        public int NumberOfInstances { get; set; }
        
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
        // Sets any NEW consumers only to earliest cursor position (can't be changed for existing subscription)
        public bool PulsarStartNewConsumerEarliest { get; set; }
    }
}
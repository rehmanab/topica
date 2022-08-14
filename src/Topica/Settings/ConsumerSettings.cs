namespace Topica.Settings
{
    public class ConsumerSettings
    {
        public static string SectionName => "ConsumerSettings";
        
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
    }
}
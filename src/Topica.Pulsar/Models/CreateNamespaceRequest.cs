using System.Collections.Generic;
using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class CreateNamespaceRequest
    {
        [JsonProperty("auth_policies")] public AuthPolicies AuthPolicies { get; set; }

        [JsonProperty("replication_clusters")] public List<string> ReplicationClusters { get; set; }

        [JsonProperty("bundles")] public Bundles Bundles { get; set; }

        [JsonProperty("backlog_quota_map")] public BacklogQuotaMap BacklogQuotaMap { get; set; }

        [JsonProperty("clusterDispatchRate")] public DispatchRate ClusterDispatchRate { get; set; }

        [JsonProperty("topicDispatchRate")] public DispatchRate TopicDispatchRate { get; set; }

        [JsonProperty("subscriptionDispatchRate")]
        public DispatchRate SubscriptionDispatchRate { get; set; }

        [JsonProperty("replicatorDispatchRate")]
        public DispatchRate ReplicatorDispatchRate { get; set; }

        [JsonProperty("clusterSubscribeRate")] public ClusterSubscribeRate ClusterSubscribeRate { get; set; }

        [JsonProperty("persistence")] public Persistence Persistence { get; set; }

        [JsonProperty("deduplicationEnabled")] public bool DeduplicationEnabled { get; set; }

        [JsonProperty("autoTopicCreationOverride")]
        public AutoTopicCreationOverride AutoTopicCreationOverride { get; set; }

        [JsonProperty("autoSubscriptionCreationOverride")]
        public AutoSubscriptionCreationOverride AutoSubscriptionCreationOverride { get; set; }

        [JsonProperty("publishMaxMessageRate")]
        public PublishMaxMessageRate PublishMaxMessageRate { get; set; }

        [JsonProperty("latency_stats_sample_rate")]
        public LatencyStatsSampleRate LatencyStatsSampleRate { get; set; }

        [JsonProperty("message_ttl_in_seconds")]
        public long MessageTtlInSeconds { get; set; }

        [JsonProperty("subscription_expiration_time_minutes")]
        public long SubscriptionExpirationTimeMinutes { get; set; }

        [JsonProperty("retention_policies")] public RetentionPolicies RetentionPolicies { get; set; }

        [JsonProperty("deleted")] public bool Deleted { get; set; }

        [JsonProperty("encryption_required")] public bool EncryptionRequired { get; set; }

        [JsonProperty("delayed_delivery_policies")]
        public DelayedDeliveryPolicies DelayedDeliveryPolicies { get; set; }

        [JsonProperty("inactive_topic_policies")]
        public InactiveTopicPolicies InactiveTopicPolicies { get; set; }

        [JsonProperty("subscription_auth_mode")]
        public string SubscriptionAuthMode { get; set; }

        [JsonProperty("max_producers_per_topic")]
        public long MaxProducersPerTopic { get; set; }

        [JsonProperty("max_consumers_per_topic")]
        public long MaxConsumersPerTopic { get; set; }

        [JsonProperty("max_consumers_per_subscription")]
        public long MaxConsumersPerSubscription { get; set; }

        [JsonProperty("max_unacked_messages_per_consumer")]
        public long MaxUnackedMessagesPerConsumer { get; set; }

        [JsonProperty("max_unacked_messages_per_subscription")]
        public long MaxUnackedMessagesPerSubscription { get; set; }

        [JsonProperty("max_subscriptions_per_topic")]
        public long MaxSubscriptionsPerTopic { get; set; }

        [JsonProperty("compaction_threshold")] public long CompactionThreshold { get; set; }

        [JsonProperty("offload_threshold")] public long OffloadThreshold { get; set; }

        [JsonProperty("offload_deletion_lag_ms")]
        public long OffloadDeletionLagMs { get; set; }

        [JsonProperty("max_topics_per_namespace")]
        public long MaxTopicsPerNamespace { get; set; }

        [JsonProperty("schema_auto_update_compatibility_strategy")]
        public string SchemaAutoUpdateCompatibilityStrategy { get; set; }

        [JsonProperty("schema_compatibility_strategy")]
        public string SchemaCompatibilityStrategy { get; set; }

        [JsonProperty("is_allow_auto_update_schema")]
        public bool IsAllowAutoUpdateSchema { get; set; }

        [JsonProperty("schema_validation_enforced")]
        public bool SchemaValidationEnforced { get; set; }

        [JsonProperty("offload_policies")] public OffloadPolicies OffloadPolicies { get; set; }

        [JsonProperty("deduplicationSnapshotIntervalSeconds")]
        public long DeduplicationSnapshotIntervalSeconds { get; set; }

        [JsonProperty("subscription_types_enabled")]
        public List<string> SubscriptionTypesEnabled { get; set; }

        [JsonProperty("properties")] public PulsarProperties PulsarProperties { get; set; }

        [JsonProperty("resource_group_name")] public string ResourceGroupName { get; set; }
    }
}
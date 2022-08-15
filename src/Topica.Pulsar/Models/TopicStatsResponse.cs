using System.Collections.Generic;
using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class TopicStatsResponse
    {
        [JsonProperty("msgRateIn")] public long MsgRateIn { get; set; }

        [JsonProperty("msgThroughputIn")] public long MsgThroughputIn { get; set; }

        [JsonProperty("msgRateOut")] public long MsgRateOut { get; set; }

        [JsonProperty("msgThroughputOut")] public long MsgThroughputOut { get; set; }

        [JsonProperty("bytesInCounter")] public long BytesInCounter { get; set; }

        [JsonProperty("msgInCounter")] public long MsgInCounter { get; set; }

        [JsonProperty("bytesOutCounter")] public long BytesOutCounter { get; set; }

        [JsonProperty("msgOutCounter")] public long MsgOutCounter { get; set; }

        [JsonProperty("averageMsgSize")] public long AverageMsgSize { get; set; }

        [JsonProperty("msgChunkPublished")] public bool MsgChunkPublished { get; set; }

        [JsonProperty("storageSize")] public long StorageSize { get; set; }

        [JsonProperty("backlogSize")] public long BacklogSize { get; set; }

        [JsonProperty("offloadedStorageSize")] public long OffloadedStorageSize { get; set; }

        [JsonProperty("lastOffloadLedgerId")] public long LastOffloadLedgerId { get; set; }

        [JsonProperty("lastOffloadSuccessTimeStamp")]
        public long LastOffloadSuccessTimeStamp { get; set; }

        [JsonProperty("lastOffloadFailureTimeStamp")]
        public long LastOffloadFailureTimeStamp { get; set; }

        [JsonProperty("publishers")] public List<Publisher> Publishers { get; set; }

        [JsonProperty("waitingPublishers")] public long WaitingPublishers { get; set; }

        [JsonProperty("subscriptions")] public Replication Subscriptions { get; set; }

        [JsonProperty("replication")] public Replication Replication { get; set; }

        [JsonProperty("deduplicationStatus")] public string DeduplicationStatus { get; set; }

        [JsonProperty("nonContiguousDeletedMessagesRanges")]
        public long NonContiguousDeletedMessagesRanges { get; set; }

        [JsonProperty("nonContiguousDeletedMessagesRangesSerializedSize")]
        public long NonContiguousDeletedMessagesRangesSerializedSize { get; set; }

        [JsonProperty("compaction")] public Compaction Compaction { get; set; }
    }
}
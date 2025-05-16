using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class AutoTopicCreationOverride
    {
        [JsonProperty("topicType")]
        public string? TopicType { get; set; }

        [JsonProperty("defaultNumPartitions")]
        public long DefaultNumPartitions { get; set; }

        [JsonProperty("allowAutoTopicCreation")]
        public bool AllowAutoTopicCreation { get; set; }
    }
}
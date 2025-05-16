using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class InactiveTopicPolicies
    {
        [JsonProperty("inactiveTopicDeleteMode")]
        public string? InactiveTopicDeleteMode { get; set; }

        [JsonProperty("maxInactiveDurationSeconds")]
        public long MaxInactiveDurationSeconds { get; set; }

        [JsonProperty("deleteWhileInactive")]
        public bool DeleteWhileInactive { get; set; }
    }
}
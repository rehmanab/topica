using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class RetentionPolicies
    {
        [JsonProperty("retentionTimeInMinutes")]
        public long RetentionTimeInMinutes { get; set; }

        [JsonProperty("retentionSizeInMB")]
        public long RetentionSizeInMb { get; set; }
    }
}
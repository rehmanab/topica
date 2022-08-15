using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class Compaction
    {
        [JsonProperty("lastCompactionRemovedEventCount")]
        public long LastCompactionRemovedEventCount { get; set; }

        [JsonProperty("lastCompactionSucceedTimestamp")]
        public long LastCompactionSucceedTimestamp { get; set; }

        [JsonProperty("lastCompactionFailedTimestamp")]
        public long LastCompactionFailedTimestamp { get; set; }

        [JsonProperty("lastCompactionDurationTimeInMills")]
        public long LastCompactionDurationTimeInMills { get; set; }
    }
}
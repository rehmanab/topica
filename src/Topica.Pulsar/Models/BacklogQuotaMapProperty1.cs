using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class BacklogQuotaMapProperty1
    {
        [JsonProperty("policy")]
        public string Policy { get; set; }

        [JsonProperty("limit")]
        public long Limit { get; set; }

        [JsonProperty("limitSize")]
        public long LimitSize { get; set; }

        [JsonProperty("limitTime")]
        public long LimitTime { get; set; }
    }
}
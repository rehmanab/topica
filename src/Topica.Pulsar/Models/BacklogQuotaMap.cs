using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class BacklogQuotaMap
    {
        [JsonProperty("property1")]
        public BacklogQuotaMapProperty1 Property1 { get; set; }

        [JsonProperty("property2")]
        public BacklogQuotaMapProperty1 Property2 { get; set; }
    }
}
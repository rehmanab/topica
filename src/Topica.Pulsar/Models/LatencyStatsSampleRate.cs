using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class LatencyStatsSampleRate
    {
        [JsonProperty("property1")]
        public long Property1 { get; set; }

        [JsonProperty("property2")]
        public long Property2 { get; set; }
    }
}
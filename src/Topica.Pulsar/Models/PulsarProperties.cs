using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class PulsarProperties
    {
        [JsonProperty("property1")]
        public string? Property1 { get; set; }

        [JsonProperty("property2")]
        public string? Property2 { get; set; }
    }
}
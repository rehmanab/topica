using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class DispatchRate
    {
        [JsonProperty("property1")]
        public ClusterDispatchRateProperty1 Property1 { get; set; }

        [JsonProperty("property2")]
        public ClusterDispatchRateProperty1 Property2 { get; set; }
    }
}
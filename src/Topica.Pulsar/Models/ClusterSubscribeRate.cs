using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class ClusterSubscribeRate
    {
        [JsonProperty("property1")]
        public ClusterSubscribeRateProperty1 Property1 { get; set; }

        [JsonProperty("property2")]
        public ClusterSubscribeRateProperty1 Property2 { get; set; }
    }
}
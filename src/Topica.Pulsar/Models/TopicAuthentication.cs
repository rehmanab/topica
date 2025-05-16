using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class TopicAuthentication
    {
        [JsonProperty("property1")]
        public NamespaceAuthentication? Property1 { get; set; }

        [JsonProperty("property2")]
        public NamespaceAuthentication? Property2 { get; set; }
    }
}
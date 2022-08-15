using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class PublishMaxMessageRate
    {
        [JsonProperty("property1")]
        public PublishMaxMessageRateProperty1 Property1 { get; set; }

        [JsonProperty("property2")]
        public PublishMaxMessageRateProperty1 Property2 { get; set; }
    }
}
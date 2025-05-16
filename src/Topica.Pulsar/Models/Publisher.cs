using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class Publisher
    {
        [JsonProperty("producerId")]
        public string? ProducerId { get; set; }

        [JsonProperty("producerName")]
        public string? ProducerName { get; set; }
    }
}
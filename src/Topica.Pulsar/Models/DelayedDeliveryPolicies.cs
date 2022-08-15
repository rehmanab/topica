using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class DelayedDeliveryPolicies
    {
        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("tickTime")]
        public long TickTime { get; set; }
    }
}
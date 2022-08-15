using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class ClusterSubscribeRateProperty1
    {
        [JsonProperty("subscribeThrottlingRatePerConsumer")]
        public long SubscribeThrottlingRatePerConsumer { get; set; }

        [JsonProperty("ratePeriodInSecond")]
        public long RatePeriodInSecond { get; set; }
    }
}
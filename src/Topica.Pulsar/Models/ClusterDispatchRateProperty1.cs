using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class ClusterDispatchRateProperty1
    {
        [JsonProperty("dispatchThrottlingRateInMsg")]
        public long DispatchThrottlingRateInMsg { get; set; }

        [JsonProperty("dispatchThrottlingRateInByte")]
        public long DispatchThrottlingRateInByte { get; set; }

        [JsonProperty("relativeToPublishRate")]
        public bool RelativeToPublishRate { get; set; }

        [JsonProperty("ratePeriodInSecond")]
        public long RatePeriodInSecond { get; set; }
    }
}
using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class PublishMaxMessageRateProperty1
    {
        [JsonProperty("publishThrottlingRateInMsg")]
        public long PublishThrottlingRateInMsg { get; set; }

        [JsonProperty("publishThrottlingRateInByte")]
        public long PublishThrottlingRateInByte { get; set; }
    }
}
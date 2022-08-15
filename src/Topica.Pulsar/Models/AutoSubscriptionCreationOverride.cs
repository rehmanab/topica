using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class AutoSubscriptionCreationOverride
    {
        [JsonProperty("allowAutoSubscriptionCreation")]
        public bool AllowAutoSubscriptionCreation { get; set; }
    }
}
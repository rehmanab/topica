using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class AuthPolicies
    {
        [JsonProperty("subscriptionAuthentication")]
        public NamespaceAuthentication? SubscriptionAuthentication { get; set; }

        [JsonProperty("namespaceAuthentication")]
        public NamespaceAuthentication? NamespaceAuthentication { get; set; }

        [JsonProperty("topicAuthentication")]
        public TopicAuthentication? TopicAuthentication { get; set; }
    }
}
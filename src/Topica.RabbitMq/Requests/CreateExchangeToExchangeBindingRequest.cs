using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Topica.RabbitMq.Requests
{
    public class CreateExchangeToExchangeBindingRequest
    {
        public string SourceExchangeName { get; set; }
        public string DestinationExchangeName { get; set; }

        [JsonPropertyName("routing_key")]
        public string RoutingKey { get; set; }

        public IDictionary<string, object> Arguments { get; set; }
    }
}
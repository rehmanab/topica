using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Topica.RabbitMq.Requests
{
    public class CreateExchangeQueueBindingRequest
    {
        public string ExchangeName { get; set; }
        public string QueueName { get; set; }
	
        [JsonPropertyName("routing_key")]
        public string RoutingKey { get; set; }
	
        public IDictionary<string, object> Arguments { get; set; }
    }
}
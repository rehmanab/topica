using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Topica.RabbitMq.Models
{
    public class Binding
    {
        public string? Source { get; set; }
        public string? VHost { get; set; }
        public string? Destination { get; set; }
	
        [JsonPropertyName("destination_type")]
        public string? DestinationType { get; set; }

        [JsonPropertyName("routing_key")]
        public string? RoutingKey { get; set; }
	
        public IDictionary<string, object>? Arguments { get; set; }
	
        [JsonPropertyName("properties_key")]
        public string? PropertiesKey { get; set; }	
    }
}
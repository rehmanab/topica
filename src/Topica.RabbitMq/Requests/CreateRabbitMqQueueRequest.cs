using System.Text.Json.Serialization;

namespace Topica.RabbitMq.Requests
{
    public class CreateRabbitMqQueueRequest
    {
        public string? Name { get; set; }
        public bool Durable { get; set; }

        [JsonPropertyName("routing_key")]
        public string? RoutingKey { get; set; }
    }
}
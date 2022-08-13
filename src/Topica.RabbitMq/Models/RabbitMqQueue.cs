using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Topica.RabbitMq.Models
{
    public class RabbitMqQueue
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string State { get; set; }
        public string Node { get; set; }
        public bool Durable { get; set; }
        public bool Exclusive { get; set; }
        public string VHost { get; set; }
        public IDictionary<string, object> Arguments { get; set; }

        [JsonPropertyName("auto_delete")]
        public bool AutoDelete { get; set; }
    }
}
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Topica.RabbitMq.Models
{
    public class Exchange
    {
        public string Name { get; set; }
	
        [JsonConverter(typeof(JsonStringEnumConverter))] 
        public ExchangeTypes Type { get; set; }
	
        public bool Durable { get; set; }
        public bool Internal { get; set; }
        public IDictionary<string, object> Arguments { get; set; }
        public string VHost { get; set; }
	
        [JsonPropertyName("user_who_performed_action")]
        public string UserWhoPerformedAction { get; set; }

        [JsonPropertyName("auto_delete")]
        public bool AutoDelete { get; set; }
    }
}
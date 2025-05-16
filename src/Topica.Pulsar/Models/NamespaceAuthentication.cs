using System.Collections.Generic;
using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class NamespaceAuthentication
    {
        [JsonProperty("property1")]
        public List<string>? Property1 { get; set; }

        [JsonProperty("property2")]
        public List<string>? Property2 { get; set; }
    }
}
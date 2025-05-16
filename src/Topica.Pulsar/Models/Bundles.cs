using System.Collections.Generic;
using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class Bundles
    {
        [JsonProperty("boundaries")]
        public List<string>? Boundaries { get; set; }

        [JsonProperty("numBundles")]
        public long NumBundles { get; set; }
    }
}
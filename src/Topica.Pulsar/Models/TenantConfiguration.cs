using System.Collections.Generic;
using Newtonsoft.Json;

namespace Topica.Pulsar.Models;

public class TenantConfiguration
{
    [JsonProperty("adminRoles")]
    public List<string> AdminRoles { get; set; }

    [JsonProperty("allowedClusters")]
    public List<string> AllowedClusters { get; set; }
}
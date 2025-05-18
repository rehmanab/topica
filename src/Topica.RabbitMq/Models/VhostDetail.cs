using System.Collections.Generic;
using Newtonsoft.Json;

namespace Topica.RabbitMq.Models;

public class VhostDetail
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public MetadataDetail? Metadata { get; set; }
    public string[]? Tags { get; set; }
    
    [JsonProperty("default_queue_type")]
    public string? DefaultQueueType { get; set; }
    
    [JsonProperty("protected_from_deletion")]
    public bool ProtectedFromDeletion { get; set; }
    
    public bool Tracing { get; set; }
    
    [JsonProperty("cluster_state")]
    public KeyValuePair<string, string>? ClusterState { get; set; }
        
    public class MetadataDetail
    {
        public string? Description { get; set; }
        public string[]? Tags { get; set; }
        
        [JsonProperty("default_queue_type")]
        public string? DefaultQueueType { get; set; }
    }
}
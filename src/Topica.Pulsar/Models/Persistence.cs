using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class Persistence
    {
        [JsonProperty("bookkeeperEnsemble")]
        public long BookkeeperEnsemble { get; set; }

        [JsonProperty("bookkeeperWriteQuorum")]
        public long BookkeeperWriteQuorum { get; set; }

        [JsonProperty("bookkeeperAckQuorum")]
        public long BookkeeperAckQuorum { get; set; }

        [JsonProperty("managedLedgerMaxMarkDeleteRate")]
        public long ManagedLedgerMaxMarkDeleteRate { get; set; }
    }
}
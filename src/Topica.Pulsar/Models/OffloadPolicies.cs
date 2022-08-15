using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class OffloadPolicies
    {
        [JsonProperty("managedLedgerOffloadDriver")]
        public string ManagedLedgerOffloadDriver { get; set; }

        [JsonProperty("offloadersDirectory")] public string OffloadersDirectory { get; set; }

        [JsonProperty("managedLedgerOffloadMaxThreads")]
        public long ManagedLedgerOffloadMaxThreads { get; set; }

        [JsonProperty("managedLedgerOffloadThresholdInBytes")]
        public long ManagedLedgerOffloadThresholdInBytes { get; set; }

        [JsonProperty("managedLedgerOffloadDeletionLagInMillis")]
        public long ManagedLedgerOffloadDeletionLagInMillis { get; set; }

        [JsonProperty("managedLedgerOffloadPrefetchRounds")]
        public long ManagedLedgerOffloadPrefetchRounds { get; set; }

        [JsonProperty("managedLedgerOffloadedReadPriority")]
        public string ManagedLedgerOffloadedReadPriority { get; set; }

        [JsonProperty("s3ManagedLedgerOffloadRegion")]
        public string S3ManagedLedgerOffloadRegion { get; set; }

        [JsonProperty("s3ManagedLedgerOffloadBucket")]
        public string S3ManagedLedgerOffloadBucket { get; set; }

        [JsonProperty("s3ManagedLedgerOffloadServiceEndpoint")]
        public string S3ManagedLedgerOffloadServiceEndpoint { get; set; }

        [JsonProperty("s3ManagedLedgerOffloadMaxBlockSizeInBytes")]
        public long S3ManagedLedgerOffloadMaxBlockSizeInBytes { get; set; }

        [JsonProperty("s3ManagedLedgerOffloadCredentialId")]
        public string S3ManagedLedgerOffloadCredentialId { get; set; }

        [JsonProperty("s3ManagedLedgerOffloadCredentialSecret")]
        public string S3ManagedLedgerOffloadCredentialSecret { get; set; }

        [JsonProperty("s3ManagedLedgerOffloadRole")]
        public string S3ManagedLedgerOffloadRole { get; set; }

        [JsonProperty("s3ManagedLedgerOffloadRoleSessionName")]
        public string S3ManagedLedgerOffloadRoleSessionName { get; set; }

        [JsonProperty("s3ManagedLedgerOffloadReadBufferSizeInBytes")]
        public long S3ManagedLedgerOffloadReadBufferSizeInBytes { get; set; }

        [JsonProperty("gcsManagedLedgerOffloadRegion")]
        public string GcsManagedLedgerOffloadRegion { get; set; }

        [JsonProperty("gcsManagedLedgerOffloadBucket")]
        public string GcsManagedLedgerOffloadBucket { get; set; }

        [JsonProperty("gcsManagedLedgerOffloadMaxBlockSizeInBytes")]
        public long GcsManagedLedgerOffloadMaxBlockSizeInBytes { get; set; }

        [JsonProperty("gcsManagedLedgerOffloadReadBufferSizeInBytes")]
        public long GcsManagedLedgerOffloadReadBufferSizeInBytes { get; set; }

        [JsonProperty("gcsManagedLedgerOffloadServiceAccountKeyFile")]
        public string GcsManagedLedgerOffloadServiceAccountKeyFile { get; set; }

        [JsonProperty("fileSystemProfilePath")]
        public string FileSystemProfilePath { get; set; }

        [JsonProperty("fileSystemURI")] public string FileSystemUri { get; set; }

        [JsonProperty("managedLedgerOffloadBucket")]
        public string ManagedLedgerOffloadBucket { get; set; }

        [JsonProperty("managedLedgerOffloadRegion")]
        public string ManagedLedgerOffloadRegion { get; set; }

        [JsonProperty("managedLedgerOffloadServiceEndpoint")]
        public string ManagedLedgerOffloadServiceEndpoint { get; set; }

        [JsonProperty("managedLedgerOffloadMaxBlockSizeInBytes")]
        public long ManagedLedgerOffloadMaxBlockSizeInBytes { get; set; }

        [JsonProperty("managedLedgerOffloadReadBufferSizeInBytes")]
        public long ManagedLedgerOffloadReadBufferSizeInBytes { get; set; }
    }
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Topica.Pulsar.Models;

namespace Topica.Pulsar.Contracts
{
    public interface IPulsarService
    {
        Task<bool> CreateDefaultUserAsync(CancellationToken cancellationToken);
        Task<IEnumerable<string>> GetClustersAsync(CancellationToken cancellationToken);
        Task<ClusterConfiguration?> GetClusterAsync(string clusterName, CancellationToken cancellationToken);
        Task<IEnumerable<string>> GetTenantsAsync(CancellationToken cancellationToken);
        Task<TenantConfiguration?> GetTenantAsync(string tenantName, CancellationToken cancellationToken);
        Task CreateTenantAsync(string tenantName, CancellationToken cancellationToken);
        Task DeleteTenantAsync(string tenantName, CancellationToken cancellationToken);
        Task<IEnumerable<string>> GetNamespacesAsync(string tenant, CancellationToken cancellationToken);
        Task CreateNamespaceAsync(string tenant, string @namespace, CancellationToken cancellationToken);
        Task DeleteNamespaceAsync(string tenant, string @namespace,CancellationToken cancellationToken);
        Task<IEnumerable<string>> GetTopicsAsync(string tenant, string @namespace, bool isPersistent = true, bool isPartitioned = false, CancellationToken cancellationToken = default);
        Task<PartitionedTopicMetaData?> GetPartitionTopicMetaDataAsync(string tenant, string @namespace, string topicName, bool isPersistent = true, CancellationToken cancellationToken = default);
        Task CreateNonPartitionedTopicAsync(string tenant, string @namespace, string topicName, bool isPersistent = true, CancellationToken cancellationToken = default);
        Task CreatePartitionedTopicAsync(string tenant, string @namespace, string topicName, int numberOfPartitions, bool isPersistent = true, CancellationToken cancellationToken = default);
        Task CreateMissedPartitionsTopicAsync(string tenant, string @namespace, string topicName, bool isPersistent = true, CancellationToken cancellationToken = default);
        Task UpdatePartitionedTopicAsync(string tenant, string @namespace, string topicName, int numberOfPartitions, bool isPersistent = true, CancellationToken cancellationToken = default);
        Task TerminatePersistentTopicAsync(string tenant, string @namespace, string topicName, CancellationToken cancellationToken);
        Task DeleteTopicAsync(string tenant, string @namespace, string topicName, bool isPersistent = true, bool isPartitioned = false, CancellationToken cancellationToken = default);
        Task<TopicStatsResponse> GetTopicsStatsAsync(string tenant, string @namespace, string topicName, bool isPersistent = true, CancellationToken cancellationToken = default);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Topica.Pulsar.Models;

namespace Topica.Pulsar.Contracts
{
    public interface IPulsarService
    {
        Task<bool> CreateDefaultUserAsync();
        Task<IEnumerable<string>> GetNamespacesAsync(string tenant);
        Task CreateNamespaceAsync(string tenant, string @namespace);
        Task DeleteNamespaceAsync(string tenant, string @namespace);
        Task<IEnumerable<string>> GetTopicsAsync(string tenant, string @namespace, bool isPersistent = true);
        Task CreateTopicAsync(string tenant, string @namespace, string topicName, bool isPersistent = true);
        Task TerminatePersistentTopicAsync(string tenant, string @namespace, string topicName);
        Task DeleteTopicAsync(string tenant, string @namespace, string topicName, bool isPersistent = true);
        Task<TopicStatsResponse> GetTopicsStatsAsync(string tenant, string @namespace, string topicName, bool isPersistent = true);
    }
}
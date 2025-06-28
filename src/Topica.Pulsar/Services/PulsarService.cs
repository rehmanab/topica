using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Topica.Contracts;
using Topica.Pulsar.Contracts;
using Topica.Pulsar.Models;

namespace Topica.Pulsar.Services
{
    public class PulsarService(string pulsarManagerBaseUrl, string pulsarAdminUrl, IHttpClientService httpClientService, ILogger<PulsarService> logger) : IPulsarService
    {
        // This is for Pulsar Manager - the Web Frontend (http://localhost:9527/#/environments) - Tasks a minute to start when docker fresh container created
        public async Task<bool> CreateDefaultUserAsync(CancellationToken cancellationToken)
        {
            var token = await httpClientService.GetAsync($"{pulsarManagerBaseUrl}/pulsar-manager/csrf-token", cancellationToken);
            httpClientService.AddHeader("X-XSRF-TOKEN", token);

            var createUser = new CreateUserRequest { Name = "admin", Password = "apachepulsar", Description = "Admin user", Email = "test@test.com" };

            var result = await httpClientService.PutAsync<CreateUserRequest, CreateUserResponse>($"{pulsarManagerBaseUrl}/pulsar-manager/users/superuser", createUser, cancellationToken);

            if (!string.IsNullOrWhiteSpace(result.Error) && !result.Error.StartsWith("Super user role is exist"))
            {
                throw new ApplicationException($"Error creating super user: {result.Error}");
            }

            return true;
        }

        // Clusters
        public async Task<IEnumerable<string>> GetClustersAsync(CancellationToken cancellationToken)
        {
            httpClientService.ClearHeaders();

            var response = await httpClientService.GetAsync<IEnumerable<string>>($"{pulsarAdminUrl}/admin/v2/clusters", cancellationToken);

            return response;
        }

        public async Task<ClusterConfiguration?> GetClusterAsync(string clusterName, CancellationToken cancellationToken)
        {
            httpClientService.ClearHeaders();

            var response = await httpClientService.GetHttpResponseMessageAsync($"{pulsarAdminUrl}/admin/v2/clusters/{clusterName}", cancellationToken);

            var message = await response.Content.ReadAsStringAsync(cancellationToken);

            // Tenant does not exist
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<ClusterConfiguration>(message);
            }

            var error = JsonConvert.DeserializeObject<ClusterErrorResponse>(message);
            return error != null && error.Reason.ToLower() == "cluster does not exist"
                ? null
                : throw new ApplicationException($"{response.StatusCode} : {response.ReasonPhrase}");
        }

        // Tenants
        public async Task<IEnumerable<string>> GetTenantsAsync(CancellationToken cancellationToken)
        {
            httpClientService.ClearHeaders();

            var response = await httpClientService.GetAsync<IEnumerable<string>>($"{pulsarAdminUrl}/admin/v2/tenants", cancellationToken);

            return response;
        }

        public async Task<TenantConfiguration?> GetTenantAsync(string tenantName, CancellationToken cancellationToken)
        {
            httpClientService.ClearHeaders();

            var response = await httpClientService.GetHttpResponseMessageAsync($"{pulsarAdminUrl}/admin/v2/tenants/{tenantName}", cancellationToken);

            var message = await response.Content.ReadAsStringAsync(cancellationToken);

            // Tenant does not exist
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<TenantConfiguration>(message);
            }

            var error = JsonConvert.DeserializeObject<TenantErrorResponse>(message);
            return error != null && error.Reason.ToLower() == "tenant does not exist"
                ? null
                : throw new ApplicationException($"{response.StatusCode} : {response.ReasonPhrase}");
        }

        public async Task CreateTenantAsync(string tenantName, CancellationToken cancellationToken)
        {
            var tenant = await GetTenantAsync(tenantName, cancellationToken);

            if (tenant != null) return;

            var tenantConfigurationRequest = new TenantConfiguration { AdminRoles = ["Admin"], AllowedClusters = (await GetClustersAsync(cancellationToken)).ToList() };
            var createResponse = await httpClientService.PutAsync($"{pulsarAdminUrl}/admin/v2/tenants/{tenantName}", tenantConfigurationRequest, cancellationToken);

            createResponse.EnsureSuccessStatusCode();
        }

        public async Task DeleteTenantAsync(string tenantName, CancellationToken cancellationToken)
        {
            var tenant = await GetTenantAsync(tenantName, cancellationToken);

            if (tenant == null) return;

            httpClientService.ClearHeaders();
            var response = await httpClientService.DeleteAsync($"{pulsarAdminUrl}/admin/v2/tenants/{tenantName}", cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        // Namespaces
        public async Task<IEnumerable<string>> GetNamespacesAsync(string tenant, CancellationToken cancellationToken)
        {
            httpClientService.ClearHeaders();
            var response = await httpClientService.GetAsync<IEnumerable<string>>($"{pulsarAdminUrl}/admin/v2/namespaces/{tenant}", cancellationToken);

            return response;
        }

        public async Task CreateNamespaceAsync(string tenant, string @namespace, CancellationToken cancellationToken)
        {
            try
            {
                var namespaces = await GetNamespacesAsync(tenant, cancellationToken);

                if (namespaces.Any(x => x.EndsWith($"{tenant}/{@namespace}")))
                {
                    return;
                }

                var createResponse = await httpClientService.PutAsync<object>($"{pulsarAdminUrl}/admin/v2/namespaces/{tenant}/{@namespace}", null!, cancellationToken);

                if (!createResponse.IsSuccessStatusCode)
                {
                    throw new ApplicationException(await createResponse.Content.ReadAsStringAsync(cancellationToken));
                }

                // Add retention
                var retentionRequest = new RetentionPolicies { RetentionSizeInMb = -1, RetentionTimeInMinutes = -1 };
                var retentionResponse = await httpClientService.PostAsync<object>($"{pulsarAdminUrl}/admin/v2/namespaces/{tenant}/{@namespace}/retention", retentionRequest, cancellationToken);

                if (!retentionResponse.IsSuccessStatusCode)
                {
                    throw new ApplicationException(await retentionResponse.Content.ReadAsStringAsync(cancellationToken));
                }

                // Add Inactive topic policy
                var inactiveTopicPoliciesRequest = new InactiveTopicPolicies { DeleteWhileInactive = false, InactiveTopicDeleteMode = "delete_when_subscriptions_caught_up" };
                var inactiveTopicPoliciesResponse = await httpClientService.PostAsync<object>($"{pulsarAdminUrl}/admin/v2/namespaces/{tenant}/{@namespace}/inactiveTopicPolicies", inactiveTopicPoliciesRequest, cancellationToken);

                if (!inactiveTopicPoliciesResponse.IsSuccessStatusCode)
                {
                    throw new ApplicationException(await inactiveTopicPoliciesResponse.Content.ReadAsStringAsync(cancellationToken));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "**** ERROR: {ServiceName}: Error: {Message}", nameof(PulsarService), ex.Message);
            }
        }

        public async Task DeleteNamespaceAsync(string tenant, string @namespace,CancellationToken cancellationToken)
        {
            var namespaces = await GetNamespacesAsync(tenant, cancellationToken);

            if (!namespaces.Any(x => x.EndsWith($"{tenant}/{@namespace}")))
            {
                return;
            }

            var topics = await GetTopicsAsync(tenant, @namespace, cancellationToken: cancellationToken);
            foreach (var topic in topics)
            {
                var topicPaths = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
                await TerminatePersistentTopicAsync(tenant, @namespace, topicPaths[3], cancellationToken);
                await DeleteTopicAsync(tenant, @namespace, topicPaths[3], cancellationToken: cancellationToken);
            }

            httpClientService.ClearHeaders();
            var response = await httpClientService.DeleteAsync($"{pulsarAdminUrl}/admin/v2/namespaces/{tenant}/{@namespace}", cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        // Topics 
        public async Task<IEnumerable<string>> GetTopicsAsync(string tenant, string @namespace, bool isPersistent = true, bool isPartitioned = false, CancellationToken cancellationToken = default)
        {
            var namespaces = await GetNamespacesAsync(tenant, cancellationToken);

            if (!namespaces.Any(x => x.EndsWith($"{tenant}/{@namespace}")))
            {
                return [];
            }

            httpClientService.ClearHeaders();
            var response = await httpClientService.GetAsync<IEnumerable<string>>($"{pulsarAdminUrl}/admin/v2/{(isPersistent ? "persistent" : "non-persistent")}/{tenant}/{@namespace}{(isPartitioned ? "/partitioned" : "")}", cancellationToken);

            return response;
        }

        public async Task<PartitionedTopicMetaData?> GetPartitionTopicMetaDataAsync(string tenant, string @namespace, string topicName, bool isPersistent = true, CancellationToken cancellationToken = default)
        {
            var namespaces = await GetNamespacesAsync(tenant, cancellationToken);

            if (!namespaces.Any(x => x.EndsWith($"{tenant}/{@namespace}")))
            {
                return null;
            }

            httpClientService.ClearHeaders();

            var response = await httpClientService.GetAsync<PartitionedTopicMetaData>($"{pulsarAdminUrl}/admin/v2/{(isPersistent ? "persistent" : "non-persistent")}/{tenant}/{@namespace}/{topicName}/partitions", cancellationToken);

            return response;
        }

        public async Task CreateNonPartitionedTopicAsync(string tenant, string @namespace, string topicName, bool isPersistent = true, CancellationToken cancellationToken = default)
        {
            var topics = await GetTopicsAsync(tenant, @namespace, isPersistent, cancellationToken: cancellationToken);

            if (topics.Any(x => x.EndsWith($"/{tenant}/{@namespace}/{topicName}")))
            {
                return;
            }

            var createResponse = await httpClientService.PutAsync($"{pulsarAdminUrl}/admin/v2/{(isPersistent ? "persistent" : "non-persistent")}/{tenant}/{@namespace}/{topicName}", null, cancellationToken);

            createResponse.EnsureSuccessStatusCode();
        }

        public async Task CreatePartitionedTopicAsync(string tenant, string @namespace, string topicName, int numberOfPartitions, bool isPersistent = true, CancellationToken cancellationToken = default)
        {
            var topics = await GetTopicsAsync(tenant, @namespace, isPersistent, isPartitioned: true, cancellationToken: cancellationToken);

            if (topics.Any(x => x.EndsWith($"/{tenant}/{@namespace}/{topicName}")))
            {
                return;
            }

            var createResponse = await httpClientService.PutAsync($"{pulsarAdminUrl}/admin/v2/{(isPersistent ? "persistent" : "non-persistent")}/{tenant}/{@namespace}/{topicName}/partitions", numberOfPartitions, cancellationToken);

            createResponse.EnsureSuccessStatusCode();
        }

        // When topic auto-creation is disabled, and you have a partitioned topic without any partitions, you can use the create-missed-partitions command to create partitions for the topic.
        public async Task CreateMissedPartitionsTopicAsync(string tenant, string @namespace, string topicName, bool isPersistent = true, CancellationToken cancellationToken = default)
        {
            var topics = await GetTopicsAsync(tenant, @namespace, isPersistent, isPartitioned: true, cancellationToken: cancellationToken);

            if (topics.Any(x => x.EndsWith($"/{tenant}/{@namespace}/{topicName}")))
            {
                return;
            }

            var createResponse = await httpClientService.PostAsync($"{pulsarAdminUrl}/admin/v2/{(isPersistent ? "persistent" : "non-persistent")}/{tenant}/{@namespace}/{topicName}/createMissedPartitions", null, cancellationToken);

            createResponse.EnsureSuccessStatusCode();
        }

        // You can only increase the number of partitions
        public async Task UpdatePartitionedTopicAsync(string tenant, string @namespace, string topicName, int numberOfPartitions, bool isPersistent = true, CancellationToken cancellationToken = default)
        {
            var topics = await GetTopicsAsync(tenant, @namespace, isPersistent, isPartitioned: true, cancellationToken: cancellationToken);

            if (topics.Any(x => x.EndsWith($"/{tenant}/{@namespace}/{topicName}")))
            {
                return;
            }

            var response = await httpClientService.PostAsync($"{pulsarAdminUrl}/admin/v2/{(isPersistent ? "persistent" : "non-persistent")}/{tenant}/{@namespace}/{topicName}/partitions", numberOfPartitions, cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        public async Task TerminatePersistentTopicAsync(string tenant, string @namespace, string topicName, CancellationToken cancellationToken)
        {
            var topics = await GetTopicsAsync(tenant, @namespace, cancellationToken: cancellationToken);

            if (!topics.Any(x => x.EndsWith($"/{tenant}/{@namespace}/{topicName}")))
            {
                return;
            }

            httpClientService.ClearHeaders();
            var response = await httpClientService.PostAsync($"{pulsarAdminUrl}/admin/v2/persistent/{tenant}/{@namespace}/{topicName}/terminate", null, cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteTopicAsync(string tenant, string @namespace, string topicName, bool isPersistent = true, bool isPartitioned = false, CancellationToken cancellationToken = default)
        {
            var topics = await GetTopicsAsync(tenant, @namespace, isPersistent: isPersistent, isPartitioned: isPartitioned, cancellationToken: cancellationToken);

            if (!topics.Any(x => x.EndsWith($"/{tenant}/{@namespace}/{topicName}")))
            {
                return;
            }

            httpClientService.ClearHeaders();
            var response = await httpClientService.DeleteAsync($"{pulsarAdminUrl}/admin/v2/{(isPersistent ? "persistent" : "non-persistent")}/{tenant}/{@namespace}/{topicName}{(isPartitioned ? "/partitions" : "")}", cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        public async Task<TopicStatsResponse> GetTopicsStatsAsync(string tenant, string @namespace, string topicName, bool isPersistent = true, CancellationToken cancellationToken = default)
        {
            httpClientService.ClearHeaders();
            var response = await httpClientService.GetAsync<TopicStatsResponse>($"{pulsarAdminUrl}/admin/v2/{(isPersistent ? "persistent" : "non-persistent")}/{tenant}/{@namespace}/{topicName}/stats", cancellationToken);

            return response;
        }
    }
}
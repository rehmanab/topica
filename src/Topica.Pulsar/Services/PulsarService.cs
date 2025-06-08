using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Pulsar.Contracts;
using Topica.Pulsar.Models;

namespace Topica.Pulsar.Services
{
    public class PulsarService(string pulsarManagerBaseUrl, string pulsarAdminUrl, IHttpClientService httpClientService, ILogger<PulsarService> logger) : IPulsarService
    {
        public async Task<bool> CreateDefaultUserAsync()
        {
            var token = await httpClientService.GetAsync($"{pulsarManagerBaseUrl}/pulsar-manager/csrf-token");
            httpClientService.AddHeader("X-XSRF-TOKEN", token);

            var createUser = new CreateUserRequest { Name = "admin", Password = "apachepulsar", Description = "Admin user", Email = "test@test.com" };

            var result = await httpClientService.PutAsync<CreateUserRequest, CreateUserResponse>($"{pulsarManagerBaseUrl}/pulsar-manager/users/superuser", createUser);

            if (!string.IsNullOrWhiteSpace(result.Error) && !result.Error.StartsWith("Super user role is exist"))
            {
                throw new ApplicationException($"Error creating super user: {result.Error}");
            }

            return true;
        }

        // Namespaces
        public async Task<IEnumerable<string>> GetNamespacesAsync(string tenant)
        {
            httpClientService.ClearHeaders();
            var response = await httpClientService.GetAsync<IEnumerable<string>>($"{pulsarAdminUrl}/admin/v2/namespaces/{tenant}");

            return response;
        }

        public async Task CreateNamespaceAsync(string tenant, string @namespace)
        {
            try
            {
                var namespaces = await GetNamespacesAsync(tenant);

                if (namespaces.Any(x => x.EndsWith($"{tenant}/{@namespace}")))
                {
                    return;
                }

                var createResponse = await httpClientService.PutAsync<object>($"{pulsarAdminUrl}/admin/v2/namespaces/{tenant}/{@namespace}", null!);

                if (!createResponse.IsSuccessStatusCode)
                {
                    throw new ApplicationException(await createResponse.Content.ReadAsStringAsync());
                }

                // Add retention
                var retentionRequest = new RetentionPolicies { RetentionSizeInMb = -1, RetentionTimeInMinutes = -1 };
                var retentionResponse = await httpClientService.PostAsync<object>($"{pulsarAdminUrl}/admin/v2/namespaces/{tenant}/{@namespace}/retention", retentionRequest);

                if (!retentionResponse.IsSuccessStatusCode)
                {
                    throw new ApplicationException(await retentionResponse.Content.ReadAsStringAsync());
                }

                // Add Inactive topic policy
                var inactiveTopicPoliciesRequest = new InactiveTopicPolicies { DeleteWhileInactive = false, InactiveTopicDeleteMode = "delete_when_subscriptions_caught_up" };
                var inactiveTopicPoliciesResponse = await httpClientService.PostAsync<object>($"{pulsarAdminUrl}/admin/v2/namespaces/{tenant}/{@namespace}/inactiveTopicPolicies", inactiveTopicPoliciesRequest);

                if (!inactiveTopicPoliciesResponse.IsSuccessStatusCode)
                {
                    throw new ApplicationException(await inactiveTopicPoliciesResponse.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "**** ERROR: {ServiceName}: Error: {Message}", nameof(PulsarService), ex.Message);
            }
        }

        public async Task DeleteNamespaceAsync(string tenant, string @namespace)
        {
            var namespaces = await GetNamespacesAsync(tenant);

            if (!namespaces.Any(x => x.EndsWith($"{tenant}/{@namespace}")))
            {
                return;
            }

            var topics = await GetTopicsAsync(tenant, @namespace);
            foreach (var topic in topics)
            {
                var topicPaths = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
                await TerminatePersistentTopicAsync(tenant, @namespace, topicPaths[3]);
                await DeleteTopicAsync(tenant, @namespace, topicPaths[3]);
            }

            httpClientService.ClearHeaders();
            var response = await httpClientService.DeleteAsync($"{pulsarAdminUrl}/admin/v2/namespaces/{tenant}/{@namespace}");

            response.EnsureSuccessStatusCode();
        }

        // Topics 
        public async Task<IEnumerable<string>> GetTopicsAsync(string tenant, string @namespace, bool isPersistent = true, bool isPartitioned = false)
        {
            var namespaces = await GetNamespacesAsync(tenant);

            if (!namespaces.Any(x => x.EndsWith($"{tenant}/{@namespace}")))
            {
                return [];
            }

            httpClientService.ClearHeaders();
            var response = await httpClientService.GetAsync<IEnumerable<string>>($"{pulsarAdminUrl}/admin/v2/{(isPersistent ? "persistent" : "non-persistent")}/{tenant}/{@namespace}{(isPartitioned ? "/partitioned" : "")}");

            return response;
        }
        
        public async Task<PartitionedTopicMetaData?> GetPartitionTopicMetaDataAsync(string tenant, string @namespace, string topicName, bool isPersistent = true)
        {
            var namespaces = await GetNamespacesAsync(tenant);

            if (!namespaces.Any(x => x.EndsWith($"{tenant}/{@namespace}")))
            {
                return null;
            }

            httpClientService.ClearHeaders();

            var response = await httpClientService.GetAsync<PartitionedTopicMetaData>($"{pulsarAdminUrl}/admin/v2/{(isPersistent ? "persistent" : "non-persistent")}/{tenant}/{@namespace}/{topicName}/partitions");

            return response;
        }

        public async Task CreateNonPartitionedTopicAsync(string tenant, string @namespace, string topicName, bool isPersistent = true)
        {
            var topics = await GetTopicsAsync(tenant, @namespace, isPersistent);

            if (topics.Any(x => x.EndsWith($"/{tenant}/{@namespace}/{topicName}")))
            {
                return;
            }

            var createResponse = await httpClientService.PutAsync($"{pulsarAdminUrl}/admin/v2/{(isPersistent ? "persistent" : "non-persistent")}/{tenant}/{@namespace}/{topicName}", null);

            createResponse.EnsureSuccessStatusCode();
        }
        
        public async Task CreatePartitionedTopicAsync(string tenant, string @namespace, string topicName, int numberOfPartitions, bool isPersistent = true)
        {
            var topics = await GetTopicsAsync(tenant, @namespace, isPersistent, isPartitioned: true);

            if (topics.Any(x => x.EndsWith($"/{tenant}/{@namespace}/{topicName}")))
            {
                return;
            }

            var createResponse = await httpClientService.PutAsync($"{pulsarAdminUrl}/admin/v2/{(isPersistent ? "persistent" : "non-persistent")}/{tenant}/{@namespace}/{topicName}/partitions", numberOfPartitions);

            createResponse.EnsureSuccessStatusCode();
        }
        
        // When topic auto-creation is disabled, and you have a partitioned topic without any partitions, you can use the create-missed-partitions command to create partitions for the topic.
        public async Task CreateMissedPartitionsTopicAsync(string tenant, string @namespace, string topicName, bool isPersistent = true)
        {
            var topics = await GetTopicsAsync(tenant, @namespace, isPersistent, isPartitioned: true);

            if (topics.Any(x => x.EndsWith($"/{tenant}/{@namespace}/{topicName}")))
            {
                return;
            }

            var createResponse = await httpClientService.PostAsync($"{pulsarAdminUrl}/admin/v2/{(isPersistent ? "persistent" : "non-persistent")}/{tenant}/{@namespace}/{topicName}/createMissedPartitions", (string)null);

            createResponse.EnsureSuccessStatusCode();
        }
        
        // You can only increase the number of partitions
        public async Task UpdatePartitionedTopicAsync(string tenant, string @namespace, string topicName, int numberOfPartitions, bool isPersistent = true)
        {
            var topics = await GetTopicsAsync(tenant, @namespace, isPersistent, isPartitioned: true);

            if (topics.Any(x => x.EndsWith($"/{tenant}/{@namespace}/{topicName}")))
            {
                return;
            }

            var response = await httpClientService.PostAsync($"{pulsarAdminUrl}/admin/v2/{(isPersistent ? "persistent" : "non-persistent")}/{tenant}/{@namespace}/{topicName}/partitions", numberOfPartitions);

            response.EnsureSuccessStatusCode();
        }

        public async Task TerminatePersistentTopicAsync(string tenant, string @namespace, string topicName)
        {
            var topics = await GetTopicsAsync(tenant, @namespace);

            if (!topics.Any(x => x.EndsWith($"/{tenant}/{@namespace}/{topicName}")))
            {
                return;
            }

            httpClientService.ClearHeaders();
            var response = await httpClientService.PostAsync($"{pulsarAdminUrl}/admin/v2/persistent/{tenant}/{@namespace}/{topicName}/terminate", null);

            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteTopicAsync(string tenant, string @namespace, string topicName, bool isPersistent = true, bool isPartitioned = false)
        {
            var topics = await GetTopicsAsync(tenant, @namespace, isPersistent: isPersistent, isPartitioned: isPartitioned);

            if (!topics.Any(x => x.EndsWith($"/{tenant}/{@namespace}/{topicName}")))
            {
                return;
            }

            httpClientService.ClearHeaders();
            var response = await httpClientService.DeleteAsync($"{pulsarAdminUrl}/admin/v2/{(isPersistent ? "persistent" : "non-persistent")}/{tenant}/{@namespace}/{topicName}{(isPartitioned ? "/partitions" : "")}");

            response.EnsureSuccessStatusCode();
        }

        public async Task<TopicStatsResponse> GetTopicsStatsAsync(string tenant, string @namespace, string topicName, bool isPersistent = true)
        {
            httpClientService.ClearHeaders();
            var response = await httpClientService.GetAsync<TopicStatsResponse>($"{pulsarAdminUrl}/admin/v2/{(isPersistent ? "persistent" : "non-persistent")}/{tenant}/{@namespace}/{topicName}/stats");

            return response;
        }
    }
}
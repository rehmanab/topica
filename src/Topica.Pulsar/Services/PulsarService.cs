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
    public class PulsarService : IPulsarService
    {
        private readonly string _pulsarManagerBaseUrl;
        private readonly string _pulsarAdminUrl;
        private readonly IHttpClientService _httpClientService;
        private readonly ILogger<PulsarService> _logger;

        public PulsarService(string pulsarManagerBaseUrl, string pulsarAdminUrl, IHttpClientService httpClientService, ILogger<PulsarService> logger)
        {
            _pulsarManagerBaseUrl = pulsarManagerBaseUrl;
            _pulsarAdminUrl = pulsarAdminUrl;
            _httpClientService = httpClientService;
            _logger = logger;
        }

        public async Task<bool> CreateDefaultUserAsync()
        {
            var token = await _httpClientService.GetAsync($"{_pulsarManagerBaseUrl}/pulsar-manager/csrf-token");
            _httpClientService.AddHeader("X-XSRF-TOKEN", token);

            var createUser = new CreateUserRequest { Name = "admin", Password = "apachepulsar", Description = "Admin user", Email = "test@test.com" };

            var result = await _httpClientService.PutAsync<CreateUserRequest, CreateUserResponse>($"{_pulsarManagerBaseUrl}/pulsar-manager/users/superuser", createUser);

            if (!string.IsNullOrWhiteSpace(result.Error) && !result.Error.StartsWith("Super user role is exist"))
            {
                throw new ApplicationException($"Error creating super user: {result.Error}");
            }

            return true;
        }

        // Namespaces
        public async Task<IEnumerable<string>> GetNamespacesAsync(string tenant)
        {
            _httpClientService.ClearHeaders();
            var response = await _httpClientService.GetAsync<IEnumerable<string>>($"{_pulsarAdminUrl}/admin/v2/namespaces/{tenant}");

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

                var createResponse = await _httpClientService.PutAsync<object>($"{_pulsarAdminUrl}/admin/v2/namespaces/{tenant}/{@namespace}", null);

                if (!createResponse.IsSuccessStatusCode)
                {
                    throw new ApplicationException(await createResponse.Content.ReadAsStringAsync());
                }

                // Add retention
                var retentionRequest = new RetentionPolicies { RetentionSizeInMb = -1, RetentionTimeInMinutes = -1 };
                var retentionResponse = await _httpClientService.PostAsync<object>($"{_pulsarAdminUrl}/admin/v2/namespaces/{tenant}/{@namespace}/retention", retentionRequest);

                if (!retentionResponse.IsSuccessStatusCode)
                {
                    throw new ApplicationException(await retentionResponse.Content.ReadAsStringAsync());
                }

                // Add Inactive topic policy
                var inactiveTopicPoliciesRequest = new InactiveTopicPolicies { DeleteWhileInactive = false, InactiveTopicDeleteMode = "delete_when_subscriptions_caught_up" };
                var inactiveTopicPoliciesResponse = await _httpClientService.PostAsync<object>($"{_pulsarAdminUrl}/admin/v2/namespaces/{tenant}/{@namespace}/inactiveTopicPolicies", inactiveTopicPoliciesRequest);

                if (!inactiveTopicPoliciesResponse.IsSuccessStatusCode)
                {
                    throw new ApplicationException(await inactiveTopicPoliciesResponse.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ServiceName}: Error: {Message}", nameof(PulsarService), ex.Message);
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

            _httpClientService.ClearHeaders();
            var response = await _httpClientService.DeleteAsync($"{_pulsarAdminUrl}/admin/v2/namespaces/{tenant}/{@namespace}");

            response.EnsureSuccessStatusCode();
        }

        // Topics 
        public async Task<IEnumerable<string>> GetTopicsAsync(string tenant, string @namespace, bool isPersistent = true)
        {
            var namespaces = await GetNamespacesAsync(tenant);

            if (!namespaces.Any(x => x.EndsWith($"{tenant}/{@namespace}")))
            {
                return Enumerable.Empty<string>();
            }

            _httpClientService.ClearHeaders();
            var response = await _httpClientService.GetAsync<IEnumerable<string>>($"{_pulsarAdminUrl}/admin/v2/{(isPersistent ? "persistent" : "non-persistent")}/{tenant}/{@namespace}");

            return response;
        }

        public async Task CreateTopicAsync(string tenant, string @namespace, string topicName, bool isPersistent = true)
        {
            var topics = await GetTopicsAsync(tenant, @namespace, isPersistent);

            if (topics.Any(x => x.EndsWith($"/{tenant}/{@namespace}/{topicName}")))
            {
                return;
            }

            var createResponse = await _httpClientService.PutAsync($"{_pulsarAdminUrl}/admin/v2/{(isPersistent ? "persistent" : "non-persistent")}/{tenant}/{@namespace}/{topicName}", null);

            createResponse.EnsureSuccessStatusCode();
        }

        public async Task TerminatePersistentTopicAsync(string tenant, string @namespace, string topicName)
        {
            var topics = await GetTopicsAsync(tenant, @namespace);

            if (!topics.Any(x => x.EndsWith($"/{tenant}/{@namespace}/{topicName}")))
            {
                return;
            }

            _httpClientService.ClearHeaders();
            var response = await _httpClientService.PostAsync($"{_pulsarAdminUrl}/admin/v2/persistent/{tenant}/{@namespace}/{topicName}/terminate", null);

            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteTopicAsync(string tenant, string @namespace, string topicName, bool isPersistent = true)
        {
            var topics = await GetTopicsAsync(tenant, @namespace);

            if (!topics.Any(x => x.EndsWith($"/{tenant}/{@namespace}/{topicName}")))
            {
                return;
            }

            _httpClientService.ClearHeaders();
            var response = await _httpClientService.DeleteAsync($"{_pulsarAdminUrl}/admin/v2/{(isPersistent ? "persistent" : "non-persistent")}/{tenant}/{@namespace}/{topicName}");

            response.EnsureSuccessStatusCode();
        }

        public async Task<TopicStatsResponse> GetTopicsStatsAsync(string tenant, string @namespace, string topicName, bool isPersistent = true)
        {
            _httpClientService.ClearHeaders();
            var response = await _httpClientService.GetAsync<TopicStatsResponse>($"{_pulsarAdminUrl}/admin/v2/{(isPersistent ? "persistent" : "non-persistent")}/{tenant}/{@namespace}/{topicName}/stats");

            return response;
        }
    }
}
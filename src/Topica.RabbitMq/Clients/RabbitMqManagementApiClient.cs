using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Topica.RabbitMq.Contracts;
using Topica.RabbitMq.Models;
using Topica.RabbitMq.Requests;

namespace Topica.RabbitMq.Clients
{
    public class RabbitMqManagementApiClient(string vhost, HttpClient httpClient) : IRabbitMqManagementApiClient
    {
        private readonly string _vhost = !string.IsNullOrWhiteSpace(vhost) ? HttpUtility.UrlEncode(vhost) : vhost;

        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private const string VhostsPath = "/vhosts";
        private const string UsersPath = "/users";
        private const string PermissionsPath = "/permissions";
        private const string NodesPath = "/nodes";
        private const string QueuesPath = "/queues";
        private const string ExchangesPath = "/exchanges";
        private const string BindingsPath = "/bindings";
        
        public async Task<IEnumerable<VhostDetail>> GetVHostsAsync()
        {
            return await SendAsync<IEnumerable<VhostDetail>>($"{VhostsPath}", HttpMethod.Get, null) ?? new List<VhostDetail>();			
        }

        public async Task<VhostDetail?> GetVHostAsync(string vhostName)
        {
            var response = await SendAsync($"{VhostsPath}/{vhostName}", HttpMethod.Get, null);
		
            if(!response.IsSuccessStatusCode)
            {
                return null;
            }

            var resultString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<VhostDetail>(resultString, _jsonSerializerOptions) ?? throw new NullReferenceException($"Null result returned from trying to deserialise - {resultString}");
        }

        public async Task CreateVHostsAsync(string vhostName, string vhostDescription)
        {
            await SendAsync($"{VhostsPath}/{vhostName}", HttpMethod.Put, new { Description = vhostDescription});
        }

        public async Task CreateVHostIfNotExistAsync()
        {
            var vhostResponse = await GetVHostAsync(_vhost);

            if (vhostResponse == null)
            {
                await CreateVHostsAsync(_vhost, $"{_vhost} created for Topica messaging");
            }
        }

        public async Task DeleteVHostsAsync(string vhostName)
        {
            await SendAsync($"{VhostsPath}/{vhostName}", HttpMethod.Delete, null);
        }

        // Get Exchange with queues and bindings
        public async Task<ExchangeBinding> GetExchangeAndBindingsAsync(string name)
        {
            var exchange = await GetExchangeAsync(name);
            var bindings = await GetExchangeInSourceBindingsAsync(name);

            return new ExchangeBinding { Exchange = exchange, Bindings = bindings };
        }

        // Create Exchange with queues and bindings
        public async Task CreateExchangeAndBindingsAsync(string exchangeName, bool durable, ExchangeTypes type, IEnumerable<CreateRabbitMqQueueRequest> queues)
        {
            await CreateExchangesAsync(type, durable, [exchangeName]);

            foreach (var queue in queues)
            {
                await CreateQueueAsync(queue.Name!, queue.Durable);
                await CreateExchangeQueueBindingAsync(new CreateExchangeQueueBindingRequest
                {
                    ExchangeName = exchangeName,
                    QueueName = queue.Name,
                    RoutingKey = queue.RoutingKey
                });
            }
        }

        // Delete Exchange with queues and bindings
        public async Task DeleteExchangeAndBindingsAsync(string[] exchangeNames)
        {
            foreach (var exchangeName in exchangeNames)
            {
                var exchangeBindings = await GetExchangeAndBindingsAsync(exchangeName);
                
                if (exchangeBindings.Exchange == null)
                {
                    throw new Exception($"Exchange '{exchangeName}' not found");
                }
                
                if (exchangeBindings.Bindings == null)
                {
                    throw new Exception($"Exchange bindings for '{exchangeName}' not found");
                }
                
                if (exchangeBindings.Bindings == null)
                {
                    throw new Exception($"Exchange bindings for '{exchangeName}' not found");
                }

                foreach (var binding in exchangeBindings.Bindings)
                {
                    if (binding.Destination == null)
                    {
                        throw new Exception($"Binding destination for '{exchangeName}' not found");
                    }
                    
                    await DeleteExchangeQueueBindingAsync(exchangeName, binding.Destination);
                    await DeleteQueuesAsync([binding.Destination]);
                }

                await DeleteExchangesAsync([exchangeName]);
            }
        }

        // Users
        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await SendAsync<IEnumerable<User>>(UsersPath, HttpMethod.Get, null) ?? new List<User>();
        }

        public async Task CreateOrUpdateUserAsync(CreateOrUpdateUserRequest body)
        {
            await SendAsync($"{UsersPath}/{body.Username}", HttpMethod.Put, body);
        }

        public async Task SetUserPermissionsAsync(SetUserPermissionsRequest body)
        {
            await SendAsync($"{PermissionsPath}/{_vhost}/{body.Username}", HttpMethod.Put, body);
        }

        // Exchanges
        public async Task<Exchange?> GetExchangeAsync(string name)
        {
            return await SendAsync<Exchange>($"{ExchangesPath}/{_vhost}/{name}", HttpMethod.Get, null);
        }

        public async Task<IEnumerable<Exchange>> GetExchangesAsync(bool includeSystemExchanges = false)
        {
            var exchanges = await SendAsync<IEnumerable<Exchange>>($"{ExchangesPath}/{_vhost}", HttpMethod.Get, null) ?? [];

            return exchanges
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Where(x => includeSystemExchanges || !x.Name!.StartsWith("amq"));
        }

        public async Task CreateExchangesAsync(ExchangeTypes exchangeType, bool durable, IEnumerable<string> exchangeNames)
        {
            var exchanges = await GetExchangesAsync();
            var names = exchanges.Select(x => x.Name).ToHashSet();

            foreach (var name in exchangeNames.Where(x => !names.Contains(x)))
            {
                await SendAsync($"{ExchangesPath}/{_vhost}/{name}", HttpMethod.Put, new { Type = exchangeType.ToString().ToLower(), Durable = durable });
            }
        }

        public async Task DeleteExchangesAsync(IEnumerable<string> names)
        {
            var exchanges = await GetExchangesAsync();
            var exchangeNames = exchanges.Select(x => x.Name).ToHashSet();

            foreach (var name in names.Where(x => exchangeNames.Contains(x)))
            {
                await SendAsync($"{ExchangesPath}/{_vhost}/{name}", HttpMethod.Delete, null);
            }
        }

        public async Task<IEnumerable<Binding>> GetExchangeInSourceBindingsAsync(string name)
        {
            return await SendAsync<IEnumerable<Binding>>($"{ExchangesPath}/{_vhost}/{name}/bindings/source", HttpMethod.Get, null) ?? new List<Binding>();
        }

        public async Task<IEnumerable<Binding>> GetExchangeInDestinationBindingsAsync(string name)
        {
            return await SendAsync<IEnumerable<Binding>>($"{ExchangesPath}/{_vhost}/{name}/bindings/destination", HttpMethod.Get, null) ?? new List<Binding>();
        }

        // Nodes
        public async Task<IEnumerable<NodeResult>> GetNodesAsync()
        {
            return await SendAsync<IEnumerable<NodeResult>>(NodesPath, HttpMethod.Get, null) ?? new List<NodeResult>();
        }

        // Queues
        public async Task<IEnumerable<RabbitMqQueue>> GetQueuesAsync()
        {
            return await SendAsync<IEnumerable<RabbitMqQueue>>($"{QueuesPath}/{_vhost}", HttpMethod.Get, null) ?? new List<RabbitMqQueue>();
        }

        public async Task CreateQueueAsync(string name, bool durable)
        {
            await SendAsync($"{QueuesPath}/{_vhost}/{name}", HttpMethod.Put, new { Durable = durable });
        }

        public async Task DeleteQueuesAsync(IEnumerable<string> names)
        {
            var queues = await GetQueuesAsync();
            var queueNames = queues.Select(x => x.Name).ToHashSet();

            foreach (var name in names.Where(x => queueNames.Contains(x)))
            {
                await SendAsync($"{QueuesPath}/{_vhost}/{name}", HttpMethod.Delete, null);
            }
        }

        // Bindings
        public async Task<IEnumerable<Binding>> GetAllBindingAsync()
        {
            return await SendAsync<IEnumerable<Binding>>($"{BindingsPath}/{_vhost}", HttpMethod.Get, null) ?? new List<Binding>();
        }

        public async Task<Binding?> GetExchangeQueueBindingAsync(string exchangeName, string queueName, string routingKey)
        {
            var bindings = await GetExchangeQueueBindingAsync(exchangeName, queueName);

            // Possible more than 1 because can have binding with same routing key but different Arguments Dictionary.
            // can call /api/bindings/_vhost/e/exchange/q/queue/props where props = {routingKey}~{hashOfArguments} e.g. Customer~0e5q5A
            // Dont know what hashing algo to use, should be in documents
            return bindings.FirstOrDefault(x => x.RoutingKey == routingKey);
        }

        public async Task<IEnumerable<Binding>> GetExchangeQueueBindingAsync(string exchangeName, string queueName)
        {
            return await SendAsync<IEnumerable<Binding>>($"{BindingsPath}/{_vhost}/e/{exchangeName}/q/{queueName}", HttpMethod.Get, null) ?? new List<Binding>();
        }

        public async Task CreateExchangeQueueBindingAsync(CreateExchangeQueueBindingRequest request)
        {
            await SendAsync($"{BindingsPath}/{_vhost}/e/{request.ExchangeName}/q/{request.QueueName}", HttpMethod.Post, request);
        }

        public async Task CreateExchangeToExchangeBindingAsync(CreateExchangeToExchangeBindingRequest request)
        {
            await SendAsync($"{BindingsPath}/{_vhost}/e/{request.SourceExchangeName}/e/{request.DestinationExchangeName}", HttpMethod.Post, request);
        }

        public async Task DeleteExchangeQueueBindingAsync(string exchangeName, string queueName, string? routingKey = null)
        {
            var bindings = new List<Binding>();

            if (routingKey != null)
            {
                var exchangeQueueBindingAsync = await GetExchangeQueueBindingAsync(exchangeName, queueName, routingKey);
                if(exchangeQueueBindingAsync != null) bindings.Add(exchangeQueueBindingAsync);
            }
            else
            {
                bindings.AddRange(await GetExchangeQueueBindingAsync(exchangeName, queueName));
            }

            foreach (var binding in bindings)
            {
                await SendAsync($"{BindingsPath}/{_vhost}/e/{binding.Source}/q/{binding.Destination}/{binding.PropertiesKey}", HttpMethod.Delete, null);
            }
        }

        // Private Methods
        private async Task<TResult?> SendAsync<TResult>(string path, HttpMethod httpMethod, object? body)
        {
            var response = await SendAsync(path, httpMethod, body);
            
            if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }

            var resultString = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<TResult>(resultString, _jsonSerializerOptions) ?? throw new NullReferenceException($"Null result returned from trying to deserialise - {resultString}");

            return result;
        }

        private async Task<HttpResponseMessage> SendAsync(string path, HttpMethod httpMethod, object? body)
        {
            var url = $"/api{path}";

            var message = new HttpRequestMessage(httpMethod, url);

            if (body != null)
            {
                var messageContentString = JsonSerializer.Serialize(body, _jsonSerializerOptions);
                message.Content = new StringContent(messageContentString);
            }

            var response = await httpClient.SendAsync(message, HttpCompletionOption.ResponseContentRead);

            // response.EnsureSuccessStatusCode();

            return response;
        }
    }
}
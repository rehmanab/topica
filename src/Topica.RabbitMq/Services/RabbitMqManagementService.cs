using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Topica.RabbitMq.Models;
using Topica.RabbitMq.Requests;
using Topica.RabbitMq.Settings;

namespace Topica.RabbitMq.Services
{
    public class RabbitMqManagementService : IRabbitMqManagementService
    {
        private readonly RabbitMqSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        private const string ApiBasePath = "/api";
        private const string UsersPath = "/users";
        private const string PermissionsPath = "/permissions";
        private const string NodesPath = "/nodes";
        private const string QueuesPath = "/queues";
        private const string ExchangesPath = "/exchanges";
        private const string BindingsPath = "/bindings";

        private readonly string _authorizationHeaderValue;

        public RabbitMqManagementService(RabbitMqSettings settings, HttpClient httpClient)
        {
            _settings = settings;
            _httpClient = httpClient;

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var basicAuthBytes = Encoding.UTF8.GetBytes($"{_settings.UserName}:{_settings.Password}");
            var basicAuthBase64 = Convert.ToBase64String(basicAuthBytes);
            _authorizationHeaderValue = $"Basic {basicAuthBase64}";
        }

        public string VHost => !string.IsNullOrWhiteSpace(_settings.VHost) ? HttpUtility.UrlEncode(_settings.VHost) : _settings.VHost;

        // Get Exchange with queues and bindings
        public async Task<ExchangeBinding> GetAsync(string name)
        {
            var exchange = await GetExchangeAsync(name);
            var bindings = await GetExchangeInSourceBindingsAsync(name);

            return new ExchangeBinding { Exchange = exchange, Bindings = bindings };
        }

        // Create Exchange with queues and bindings
        public async Task CreateAsync(string exchangeName, bool durable, ExchangeTypes type, IEnumerable<CreateRabbitMqQueueRequest> queues)
        {
            await CreateExchangesAsync(type, durable, new[] { exchangeName });

            foreach (var queue in queues)
            {
                await CreateQueueAsync(queue.Name, queue.Durable);
                await CreateExchangeQueueBindingAsync(new CreateExchangeQueueBindingRequest
                {
                    ExchangeName = exchangeName,
                    QueueName = queue.Name,
                    RoutingKey = queue.RoutingKey
                });
            }
        }

        // Delete Exchange with queues and bindings
        public async Task DeleteAsync(params string[] exchangeNames)
        {
            foreach (var exchangeName in exchangeNames)
            {
                var exchangeBindings = await GetAsync(exchangeName);

                foreach (var binding in exchangeBindings.Bindings)
                {
                    await DeleteExchangeQueueBindingAsync(exchangeName, binding.Destination);
                    await DeleteQueuesAsync(new[] { binding.Destination });
                }

                await DeleteExchangesAsync(new[] { exchangeName });
            }
        }

        // Users
        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await SendAsync<IEnumerable<User>>(UsersPath, HttpMethod.Get, null);
        }

        public async Task CreateOrUpdateUserAsync(CreateOrUpdateUserRequest body)
        {
            await SendAsync($"{UsersPath}/{body.Username}", HttpMethod.Put, body);
        }

        public async Task SetUserPermissionsAsync(SetUserPermissionsRequest body)
        {
            await SendAsync($"{PermissionsPath}/{VHost}/{body.Username}", HttpMethod.Put, body);
        }

        // Exchanges
        public async Task<Exchange> GetExchangeAsync(string name)
        {
            return await SendAsync<Exchange>($"{ExchangesPath}/{VHost}/{name}", HttpMethod.Get, null);
        }

        public async Task<IEnumerable<Exchange>> GetExchangesAsync(bool includeSystemExchanges = false)
        {
            var exchanges = await SendAsync<IEnumerable<Exchange>>($"{ExchangesPath}/{VHost}", HttpMethod.Get, null);

            return exchanges
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.Name))
                .Where(x => includeSystemExchanges || !x.Name.StartsWith("amq"));
        }

        public async Task CreateExchangesAsync(ExchangeTypes exchangeType, bool durable, IEnumerable<string> exchangeNames)
        {
            var exchanges = await GetExchangesAsync();
            var names = new HashSet<string>(exchanges.Select(x => x.Name));

            foreach (var name in exchangeNames.Where(x => !names.Contains(x)))
            {
                await SendAsync($"{ExchangesPath}/{VHost}/{name}", HttpMethod.Put, new { Type = exchangeType.ToString().ToLower(), Durable = durable });
            }
        }

        public async Task DeleteExchangesAsync(IEnumerable<string> names)
        {
            var exchanges = await GetExchangesAsync();
            var exchangeNames = new HashSet<string>(exchanges.Select(x => x.Name));

            foreach (var name in names.Where(x => exchangeNames.Contains(x)))
            {
                await SendAsync($"{ExchangesPath}/{VHost}/{name}", HttpMethod.Delete, null);
            }
        }

        public async Task<IEnumerable<Binding>> GetExchangeInSourceBindingsAsync(string name)
        {
            return await SendAsync<IEnumerable<Binding>>($"{ExchangesPath}/{VHost}/{name}/bindings/source", HttpMethod.Get, null);
        }

        public async Task<IEnumerable<Binding>> GetExchangeInDestinationBindingsAsync(string name)
        {
            return await SendAsync<IEnumerable<Binding>>($"{ExchangesPath}/{VHost}/{name}/bindings/destination", HttpMethod.Get, null);
        }

        // Nodes
        public async Task<IEnumerable<NodeResult>> GetNodesAsync()
        {
            return await SendAsync<IEnumerable<NodeResult>>(NodesPath, HttpMethod.Get, null);
        }

        // Queues
        public async Task<IEnumerable<RabbitMqQueue>> GetQueuesAsync()
        {
            return await SendAsync<IEnumerable<RabbitMqQueue>>($"{QueuesPath}/{VHost}", HttpMethod.Get, null);
        }

        public async Task CreateQueueAsync(string name, bool durable)
        {
            await SendAsync($"{QueuesPath}/{VHost}/{name}", HttpMethod.Put, new { Durable = durable });
        }

        public async Task DeleteQueuesAsync(IEnumerable<string> names)
        {
            var queues = await GetQueuesAsync();
            var queueNames = new HashSet<string>(queues.Select(x => x.Name));

            foreach (var name in names.Where(x => queueNames.Contains(x)))
            {
                await SendAsync($"{QueuesPath}/{VHost}/{name}", HttpMethod.Delete, null);
            }
        }

        // Bindings
        public async Task<IEnumerable<Binding>> GetAllBindingAsync()
        {
            return await SendAsync<IEnumerable<Binding>>($"{BindingsPath}/{VHost}", HttpMethod.Get, null);
        }

        public async Task<Binding> GetExchangeQueueBindingAsync(string exchangeName, string queueName, string routingKey)
        {
            var bindings = await GetExchangeQueueBindingAsync(exchangeName, queueName);

            // Possible more than 1 because can have binding with same routing key but different Arguments Dictionary.
            // can call /api/bindings/vhost/e/exchange/q/queue/props where props = {routingKey}~{hashOfArguments} e.g. Customer~0e5q5A
            // Dont know what hashing algo to use, should be in documents
            return bindings.FirstOrDefault(x => x.RoutingKey == routingKey);
        }

        public async Task<IEnumerable<Binding>> GetExchangeQueueBindingAsync(string exchangeName, string queueName)
        {
            return await SendAsync<IEnumerable<Binding>>($"{BindingsPath}/{VHost}/e/{exchangeName}/q/{queueName}", HttpMethod.Get, null);
        }

        public async Task CreateExchangeQueueBindingAsync(CreateExchangeQueueBindingRequest request)
        {
            await SendAsync($"{BindingsPath}/{VHost}/e/{request.ExchangeName}/q/{request.QueueName}", HttpMethod.Post, request);
        }

        public async Task CreateExchangeToExchangeBindingAsync(CreateExchangeToExchangeBindingRequest request)
        {
            await SendAsync($"{BindingsPath}/{VHost}/e/{request.SourceExchangeName}/e/{request.DestinationExchangeName}", HttpMethod.Post, request);
        }

        public async Task DeleteExchangeQueueBindingAsync(string exchangeName, string queueName, string routingKey = null)
        {
            var bindings = new List<Binding>();

            if (routingKey != null)
            {
                bindings.Add(await GetExchangeQueueBindingAsync(exchangeName, queueName, routingKey));
            }
            else
            {
                bindings.AddRange(await GetExchangeQueueBindingAsync(exchangeName, queueName));
            }

            foreach (var binding in bindings)
            {
                await SendAsync($"{BindingsPath}/{VHost}/e/{binding.Source}/q/{binding.Destination}/{binding.PropertiesKey}", HttpMethod.Delete, null);
            }
        }

        // Private Methods
        private async Task<TResult> SendAsync<TResult>(string path, HttpMethod httpMethod, object? body)
        {
            var response = await SendAsync(path, httpMethod, body);

            var resultString = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<TResult>(resultString, _jsonSerializerOptions) ?? throw new NullReferenceException($"Null result returned from trying to deserialise - {resultString}");

            return result;
        }

        private async Task<HttpResponseMessage> SendAsync(string path, HttpMethod httpMethod, object? body)
        {
            var url = $"{ApiBasePath}{path}";

            var message = new HttpRequestMessage(httpMethod, url);
            message.Headers.Add("Authorization", _authorizationHeaderValue);

            if (body != null)
            {
                var messageContentString = JsonSerializer.Serialize(body, _jsonSerializerOptions);
                message.Content = new StringContent(messageContentString);
            }

            var response = await _httpClient.SendAsync(message, HttpCompletionOption.ResponseContentRead);

            response.EnsureSuccessStatusCode();

            return response;
        }
    }
}
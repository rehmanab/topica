using System.Collections.Generic;
using System.Threading.Tasks;
using Topica.RabbitMq.Models;
using Topica.RabbitMq.Requests;

namespace Topica.RabbitMq.Clients
{
    public interface IRabbitMqManagementApiClient
    {
        Task<ExchangeBinding> GetAsync(string name);
        Task CreateAsync(string exchangeName, bool durable, ExchangeTypes type, IEnumerable<CreateRabbitMqQueueRequest> queues);
        Task DeleteAsync(params string[] exchangeNames);
        Task<IEnumerable<User>> GetUsersAsync();
        Task CreateOrUpdateUserAsync(CreateOrUpdateUserRequest body);
        Task SetUserPermissionsAsync(SetUserPermissionsRequest body);
        Task<Exchange> GetExchangeAsync(string name);
        Task<IEnumerable<Exchange>> GetExchangesAsync(bool includeSystemExchanges = false);
        Task CreateExchangesAsync(ExchangeTypes exchangeType, bool durable, IEnumerable<string> exchangeNames);
        Task DeleteExchangesAsync(IEnumerable<string> names);
        Task<IEnumerable<Binding>> GetExchangeInSourceBindingsAsync(string name);
        Task<IEnumerable<Binding>> GetExchangeInDestinationBindingsAsync(string name);
        Task<IEnumerable<NodeResult>> GetNodesAsync();
        Task<IEnumerable<RabbitMqQueue>> GetQueuesAsync();
        Task CreateQueueAsync(string name, bool durable);
        Task DeleteQueuesAsync(IEnumerable<string> names);
        Task<IEnumerable<Binding>> GetAllBindingAsync();
        Task<Binding> GetExchangeQueueBindingAsync(string exchangeName, string queueName, string routingKey);
        Task<IEnumerable<Binding>> GetExchangeQueueBindingAsync(string exchangeName, string queueName);
        Task CreateExchangeQueueBindingAsync(CreateExchangeQueueBindingRequest request);
        Task CreateExchangeToExchangeBindingAsync(CreateExchangeToExchangeBindingRequest request);
        Task DeleteExchangeQueueBindingAsync(string exchangeName, string queueName, string routingKey = null);
    }
}
using Aws.Consumer.Host.Messages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Topica.Contracts;

namespace Aws.Consumer.Host.Handlers
{
    public class OrderCreatedV1MessageHandler : IHandler<OrderCreatedV1>
    {
        private readonly ILogger<OrderCreatedV1MessageHandler> _logger;

        public OrderCreatedV1MessageHandler(ILogger<OrderCreatedV1MessageHandler> logger)
        {
            _logger = logger;
        }
        
        public async Task<bool> HandleAsync(OrderCreatedV1 source)
        {
            _logger.LogInformation(JsonConvert.SerializeObject(source));

            return await Task.FromResult(true);
        }

        public bool ValidateMessage(OrderCreatedV1 message)
        {
            return true;
        }
    }
}
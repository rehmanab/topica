using Aws.Consumer.Host.Messages;
using Microsoft.Extensions.Logging;
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
            _logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Order: {OrderName}", nameof(OrderCreatedV1), source.ConversationId, source.OrderName);


            return await Task.FromResult(true);
        }

        public bool ValidateMessage(OrderCreatedV1 message)
        {
            return true;
        }
    }
}
using Aws.Consumer.Host.Messages.V1;
using Microsoft.Extensions.Logging;
using Topica.Contracts;

namespace Aws.Consumer.Host.Handlers.V1
{
    public class OrderCreatedMessageHandler : IHandler<OrderCreatedMessage>
    {
        private readonly ILogger<OrderCreatedMessageHandler> _logger;

        public OrderCreatedMessageHandler(ILogger<OrderCreatedMessageHandler> logger)
        {
            _logger = logger;
        }
        
        public async Task<bool> HandleAsync(OrderCreatedMessage source)
        {
            _logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Order: {OrderName}", nameof(OrderCreatedMessage), source.ConversationId, source.OrderName);
            
            return await Task.FromResult(true);
        }

        public bool ValidateMessage(OrderCreatedMessage message)
        {
            return true;
        }
    }
}
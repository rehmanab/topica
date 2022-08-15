using Aws.Consumer.Host.Messages.V1;
using Microsoft.Extensions.Logging;
using Topica.Contracts;

namespace Aws.Consumer.Host.Handlers.V1
{
    public class CustomerCreatedMessageHandler : IHandler<CustomerCreatedMessage>
    {
        private readonly ILogger<CustomerCreatedMessageHandler> _logger;
        
        public CustomerCreatedMessageHandler(ILogger<CustomerCreatedMessageHandler> logger)
        {
            _logger = logger;
        }

        public async Task<bool> HandleAsync(CustomerCreatedMessage source)
        {
            _logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Customer: {CustomerName}", nameof(CustomerCreatedMessage), source.ConversationId, source.Name);
            
            return await Task.FromResult(true);
        }

        public bool ValidateMessage(CustomerCreatedMessage message)
        {
            return true;
        }
    }
}
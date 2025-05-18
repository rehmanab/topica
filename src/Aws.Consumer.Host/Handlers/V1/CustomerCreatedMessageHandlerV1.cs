using Aws.Consumer.Host.Messages.V1;
using Microsoft.Extensions.Logging;
using Topica.Contracts;

namespace Aws.Consumer.Host.Handlers.V1
{
    public class CustomerCreatedMessageHandlerV1(ILogger<CustomerCreatedMessageHandlerV1> logger) : IHandler<CustomerCreatedMessageV1>
    {
        public async Task<bool> HandleAsync(CustomerCreatedMessageV1 source)
        {
            logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Customer: {Customer}", nameof(CustomerCreatedMessageV1), source.ConversationId, $"{source.CustomerId} : {source.CustomerName}");
            
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Validate the message type equals the message name and can validate any properties for the message
        /// </summary>
        /// <param name="message"></param>
        /// <returns>True if Valid</returns>
        public bool ValidateMessage(CustomerCreatedMessageV1 message)
        {
            if(string.IsNullOrWhiteSpace(message.Type) || !message.Type.Equals(nameof(CustomerCreatedMessageV1), StringComparison.CurrentCultureIgnoreCase))
            {
                // logger.LogWarning("Invalid message type: {MessageType} for Handler: {Handler}", message.Type, nameof(CustomerCreatedMessageHandlerV1));
                return false;
            }
            
            return true;
        }
    }
}
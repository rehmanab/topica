using Aws.Consumer.Host.Messages.V1;
using Microsoft.Extensions.Logging;
using Topica.Contracts;

namespace Aws.Consumer.Host.Handlers.V1
{
    public class CustomerCreatedMessageHandler(ILogger<CustomerCreatedMessageHandler> logger) : IHandler<CustomerCreatedMessageV1>
    {
        public async Task<bool> HandleAsync(CustomerCreatedMessageV1 source)
        {
            logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Customer: {CustomerName}", nameof(CustomerCreatedMessageV1), source.ConversationId, source.Name);
            
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Validate the message type equals the message name and can validate any properties for the message
        /// </summary>
        /// <param name="message"></param>
        /// <returns>True if Valid</returns>
        public bool ValidateMessage(CustomerCreatedMessageV1 message)
        {
            return true;
        }
    }
}
using Aws.Consumer.Host.Messages.V1;
using Microsoft.Extensions.Logging;
using Topica.Contracts;

namespace Aws.Consumer.Host.Handlers.V1
{
    public class OrderCreatedMessageHandler(ILogger<OrderCreatedMessageHandler> logger) : IHandler<OrderCreatedMessageV1>
    {
        public async Task<bool> HandleAsync(OrderCreatedMessageV1 source)
        {
            logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Order: {OrderName}", nameof(OrderCreatedMessageV1), source.ConversationId, source.Name);
            
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Validate the message type equals the message name and can validate any properties for the message
        /// </summary>
        /// <param name="message"></param>
        /// <returns>True if Valid</returns>
        public bool ValidateMessage(OrderCreatedMessageV1 message)
        {
            return true;
        }
    }
}
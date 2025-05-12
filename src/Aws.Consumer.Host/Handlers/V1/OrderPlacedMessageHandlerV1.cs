using Aws.Consumer.Host.Messages.V1;
using Microsoft.Extensions.Logging;
using Topica.Contracts;

namespace Aws.Consumer.Host.Handlers.V1
{
    public class OrderPlacedMessageHandlerV1(ILogger<OrderPlacedMessageHandlerV1> logger) : IHandler<OrderPlacedMessageV1>
    {
        public async Task<bool> HandleAsync(OrderPlacedMessageV1 source)
        {
            // logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Order: {OrderName}", nameof(OrderPlacedMessageV1), source.ConversationId, source.ProductName);
            
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Validate the message type equals the message name and can validate any properties for the message
        /// </summary>
        /// <param name="message"></param>
        /// <returns>True if Valid</returns>
        public bool ValidateMessage(OrderPlacedMessageV1 message)
        {
            return true;
        }
    }
}
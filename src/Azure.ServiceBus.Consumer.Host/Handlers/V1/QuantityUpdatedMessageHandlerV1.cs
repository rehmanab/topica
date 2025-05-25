using System;
using System.Threading.Tasks;
using Azure.ServiceBus.Consumer.Host.Messages.V1;
using Microsoft.Extensions.Logging;
using Topica.Contracts;

namespace Azure.ServiceBus.Consumer.Host.Handlers.V1
{
    public class QuantityUpdatedMessageHandlerV1(ILogger<QuantityUpdatedMessageHandlerV1> logger) : IHandler<QuantityUpdatedMessageV1>
    {
        public async Task<bool> HandleAsync(QuantityUpdatedMessageV1 source)
        {
            // logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Quantity: {Quantity}", nameof(QuantityUpdatedMessageV1), source.ConversationId, $"{source.QuantityId} : {source.QuantityName}");
            
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Validate the message type equals the message name and can validate any properties for the message
        /// </summary>
        /// <param name="message"></param>
        /// <returns>True if Valid</returns>
        public bool ValidateMessage(QuantityUpdatedMessageV1 message)
        {
            if(string.IsNullOrWhiteSpace(message.Type) || !message.Type.Equals(nameof(QuantityUpdatedMessageV1), StringComparison.CurrentCultureIgnoreCase))
            {
                logger.LogWarning("Invalid message type: {MessageType} for Handler: {Handler}", message.Type, nameof(QuantityUpdatedMessageHandlerV1));
                return false;
            }
            
            return true;
        }
    }
}
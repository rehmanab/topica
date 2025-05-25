using System;
using System.Threading.Tasks;
using Azure.ServiceBus.Consumer.Host.Messages.V1;
using Microsoft.Extensions.Logging;
using Topica.Contracts;

namespace Azure.ServiceBus.Consumer.Host.Handlers.V1
{
    public class PriceSubmittedMessageHandlerV1(ILogger<PriceSubmittedMessageHandlerV1> logger) : IHandler<PriceSubmittedMessageV1>
    {
        public async Task<bool> HandleAsync(PriceSubmittedMessageV1 source)
        {
            // logger.LogInformation("Handle: {Name} from: {RaisingComponent} for CID: {ConversationId} for Price: {Price}", nameof(PriceSubmittedMessageV1), source.RaisingComponent, source.ConversationId, $"{source.PriceId} : {source.PriceName}");
            
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Validate the message type equals the message name and can validate any properties for the message
        /// </summary>
        /// <param name="message"></param>
        /// <returns>True if Valid</returns>
        public bool ValidateMessage(PriceSubmittedMessageV1 message)
        {
            if(string.IsNullOrWhiteSpace(message.Type) || !message.Type.Equals(nameof(PriceSubmittedMessageV1), StringComparison.CurrentCultureIgnoreCase))
            {
                // logger.LogWarning("Invalid message type: {MessageType} for Handler: {Handler}", message.Type, nameof(CustomerCreatedMessageHandlerV1));
                return false;
            }
            
            return true;
        }
    }
}
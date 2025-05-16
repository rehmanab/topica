using Microsoft.Extensions.Logging;
using RabbitMq.Consumer.Host.Messages.V1;
using Topica.Contracts;

namespace RabbitMq.Consumer.Host.Handlers.V1;

public class ItemDeliveredMessageHandlerV1(ILogger<ItemDeliveredMessageHandlerV1> logger) : IHandler<ItemDeliveredMessageV1>
{
    public async Task<bool> HandleAsync(ItemDeliveredMessageV1 source)
    {
        logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Handed to Resident: {HandedToResident}", nameof(ItemDeliveredMessageV1), source.ConversationId, source.ItemName);

        return await Task.FromResult(true);
    }

    public bool ValidateMessage(ItemDeliveredMessageV1 message)
    {
        if(!message.Type.Equals(nameof(ItemDeliveredMessageV1), StringComparison.CurrentCultureIgnoreCase))
        {
            // logger.LogWarning("Invalid message type: {MessageType} for Handler: {Handler}", message.Type, nameof(ItemDeliveredMessageHandlerV1));
            return false;
        }
        
        return true;
    }
}
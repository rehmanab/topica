using Microsoft.Extensions.Logging;
using RabbitMq.Consumer.Host.Messages.V1;
using Topica.Contracts;

namespace RabbitMq.Consumer.Host.Handlers.V1;

public class ItemPostedMessageHandlerV1(ILogger<ItemPostedMessageHandlerV1> logger) : IHandler<ItemPostedMessageV1>
{
    public async Task<bool> HandleAsync(ItemPostedMessageV1 source)
    {
        logger.LogInformation("Handle: {Name} for CID: {ConversationId} for PostTown: {PostTown}", nameof(ItemPostedMessageV1), source.ConversationId, source.PostboxName);
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(ItemPostedMessageV1 message)
    {
        if(string.IsNullOrWhiteSpace(message.Type) || !message.Type.Equals(nameof(ItemPostedMessageV1), StringComparison.CurrentCultureIgnoreCase))
        {
            // logger.LogWarning("Invalid message type: {MessageType} for Handler: {Handler}", message.Type, nameof(ItemPostedMessageHandlerV1));
            return false;
        }
        
        return true;
    }
}
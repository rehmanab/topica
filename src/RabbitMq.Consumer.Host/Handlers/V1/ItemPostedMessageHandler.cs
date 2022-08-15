using Microsoft.Extensions.Logging;
using RabbitMq.Consumer.Host.Messages.V1;
using Topica.Contracts;

namespace RabbitMq.Consumer.Host.Handlers.V1;

public class ItemPostedMessageHandler : IHandler<ItemPostedMessage>
{
    private readonly ILogger<ItemPostedMessageHandler> _logger;

    public ItemPostedMessageHandler(ILogger<ItemPostedMessageHandler> logger)
    {
        _logger = logger;
    }

    public async Task<bool> HandleAsync(ItemPostedMessage source)
    {
        _logger.LogInformation("Handle: {Name} for CID: {ConversationId} for PostTown: {PostTown}", nameof(ItemDeliveredMessage), source.ConversationId, source.Name);
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(ItemPostedMessage message)
    {
        return true;
    }
}
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMq.Consumer.Host.Messages.V1;
using Topica.Contracts;

namespace RabbitMq.Consumer.Host.Handlers.V1;

public class ItemDeliveredMessageHandler : IHandler<ItemDeliveredMessage>
{
    private readonly ILogger<ItemDeliveredMessageHandler> _logger;

    public ItemDeliveredMessageHandler(ILogger<ItemDeliveredMessageHandler> logger)
    {
        _logger = logger;
    }

    public async Task<bool> HandleAsync(ItemDeliveredMessage source)
    {
        // _logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Person: {PersonName}", nameof(ItemDeliveredMessage), source.ConversationId, source.);
        _logger.LogInformation(JsonConvert.SerializeObject(source));
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(ItemDeliveredMessage message)
    {
        return true;
    }
}
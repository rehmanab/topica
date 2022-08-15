using Microsoft.Extensions.Logging;
using Pulsar.Consumer.Host.Messages.V1;
using Topica.Contracts;

namespace Pulsar.Consumer.Host.Handlers.V1;

public class DataSentMessageHandler : IHandler<DataSentMessage>
{
    private readonly ILogger<DataSentMessageHandler> _logger;
        
    public DataSentMessageHandler(ILogger<DataSentMessageHandler> logger)
    {
        _logger = logger;
    }

    public async Task<bool> HandleAsync(DataSentMessage source)
    {
        _logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Data: {PersonName}", nameof(DataSentMessage), source.ConversationId, source.Name);
            
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(DataSentMessage message)
    {
        return true;
    }
}
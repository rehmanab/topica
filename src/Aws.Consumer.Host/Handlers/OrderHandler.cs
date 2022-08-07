using Aws.Consumer.Host.Messages;
using Microsoft.Extensions.Logging;
using Topica.Contracts;

namespace Aws.Consumer.Host.Handlers;

public class OrderHandler : IHandler<OrderMessage>
{
    private readonly ILogger<OrderHandler> _logger;

    public OrderHandler(ILogger<OrderHandler> logger)
    {
        _logger = logger;
    }
    
    public async Task<bool> Handle(OrderMessage message)
    {
        _logger.LogInformation($"***** Message OrderMessage: {message.Message} from {message.ConversationId}");
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(OrderMessage message)
    {
        return true;
    }
}
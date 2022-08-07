using Aws.Consumer.Host.Messages;
using Microsoft.Extensions.Logging;
using Topica.Contracts;

namespace Aws.Consumer.Host.Handlers;

public class CustomerHandler : IHandler<CustomerMessage>
{
    private readonly ILogger<CustomerHandler> _logger;

    public CustomerHandler(ILogger<CustomerHandler> logger)
    {
        _logger = logger;
    }
    
    public async Task<bool> Handle(CustomerMessage message)
    {
        _logger.LogInformation($"***** Message CustomerMessage: {message.Name} from {message.ConversationId}");
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(CustomerMessage message)
    {
        return true;
    }
}
using Topica.Aws.Messages;
using Topica.Contracts;

namespace Aws.Consumer.Host.Handlers;

public class DefaultHandler : IHandler<BaseSqsMessage>
{
    public async Task<bool> Handle(BaseSqsMessage message)
    {
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(BaseSqsMessage message)
    {
        return true;
    }
}
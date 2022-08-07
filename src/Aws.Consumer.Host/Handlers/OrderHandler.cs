using Aws.Consumer.Host.Messages;
using Topica.Contracts;

namespace Aws.Consumer.Host.Handlers;

public class OrderHandler : IHandler<OrderMessage>
{
    public async Task<bool> Handle(OrderMessage message)
    {
        Console.WriteLine($"***** Message OrderMessage: {message.Message} from {message.ConversationId}");
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(OrderMessage message)
    {
        return true;
    }
}
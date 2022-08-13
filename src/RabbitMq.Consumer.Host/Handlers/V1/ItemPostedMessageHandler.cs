using RabbitMq.Consumer.Host.Messages.V1;
using Topica.Contracts;

namespace RabbitMq.Consumer.Host.Handlers.V1;

public class ItemPostedMessageHandler : IHandler<ItemPostedMessage>
{
    public Task<bool> HandleAsync(ItemPostedMessage source)
    {
        throw new NotImplementedException();
    }

    public bool ValidateMessage(ItemPostedMessage message)
    {
        throw new NotImplementedException();
    }
}
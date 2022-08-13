using RabbitMq.Consumer.Host.Messages.V1;
using Topica.Contracts;

namespace RabbitMq.Consumer.Host.Handlers.V1;

public class ItemDeliveredMessageHandler : IHandler<ItemDeliveredMessage>
{
    public Task<bool> HandleAsync(ItemDeliveredMessage source)
    {
        throw new NotImplementedException();
    }

    public bool ValidateMessage(ItemDeliveredMessage message)
    {
        throw new NotImplementedException();
    }
}
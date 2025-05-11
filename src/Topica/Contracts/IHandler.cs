using System.Threading.Tasks;

namespace Topica.Contracts
{
    public interface IHandler
    {
    }

    public interface IHandler<in TMessage> : IHandler
    {
        Task<bool> HandleAsync(TMessage source);
        bool ValidateMessage(TMessage message);
    }
}
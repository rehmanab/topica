using System.Threading.Tasks;

namespace Topica.Contracts
{
    public interface IHandler<in TMessage>
    {
        Task<bool> HandleAsync(TMessage source);
        bool ValidateMessage(TMessage message);
    }
}
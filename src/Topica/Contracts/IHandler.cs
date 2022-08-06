using System.Threading.Tasks;

namespace Topica.Contracts
{
    public interface IHandler<in T>
    {
        Task<bool> Handle(T message);
        bool ValidateMessage(T message);
    }
}
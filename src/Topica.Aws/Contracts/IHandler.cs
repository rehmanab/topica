using System.Threading.Tasks;

namespace Topica.Aws.Contracts
{
    public interface IHandler<in T>
    {
        Task<bool> Handle(T message);
        bool ValidateMessage(T message);
    }
}
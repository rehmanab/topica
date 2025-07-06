using System.Collections.Generic;
using System.Threading.Tasks;

namespace Topica.Contracts
{
    public interface IHandler
    {
    }

    public interface IHandler<in TMessage> : IHandler
    {
        Task<bool> HandleAsync(TMessage source, Dictionary<string, string>? properties);
        bool ValidateMessage(TMessage message);
    }
}
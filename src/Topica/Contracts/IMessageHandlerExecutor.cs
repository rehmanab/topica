using System.Threading.Tasks;

namespace Topica.Contracts
{
    public interface IMessageHandlerExecutor
    {
        Task<(string?, bool)> ExecuteHandlerAsync(string messageBody);
    }
}
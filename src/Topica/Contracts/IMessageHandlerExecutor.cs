using System.Threading.Tasks;

namespace Topica.Contracts
{
    public interface IMessageHandlerExecutor
    {
        Task<(string, bool)> ExecuteHandlerAsync<T>(string messageBody) where T : IHandler;
        Task<(string, bool)> ExecuteHandlerAsync(string messageTypeName, string messageBody);
    }
}
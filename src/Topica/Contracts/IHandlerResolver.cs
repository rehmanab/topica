using System.Collections.Generic;

namespace Topica.Contracts
{
    public interface IHandlerResolver
    {
        (bool handlerFound, object handlerImpl, bool methodToValidate, object? methodToExecute) ResolveHandler(string source, Dictionary<string, string>? properties);
    }
}
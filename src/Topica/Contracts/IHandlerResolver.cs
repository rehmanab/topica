namespace Topica.Contracts
{
    public interface IHandlerResolver
    {
        (bool handlerFound, object handlerImpl, bool methodToValidate, object? methodToExecute) ResolveHandler(string source);
    }
}
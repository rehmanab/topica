namespace Topica.Contracts
{
    public interface IHandlerResolver
    {
        (bool handlerFound, object handlerImpl, object methodToValidate, object methodToExecute) ResolveHandler(string source);
    }
}
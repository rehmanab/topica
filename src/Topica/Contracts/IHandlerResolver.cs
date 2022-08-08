namespace Topica.Contracts
{
    public interface IHandlerResolver
    {
        (object handlerImpl, object methodToValidate, object methodToExecute) ResolveHandler(string handlerType, string source);
    }
}
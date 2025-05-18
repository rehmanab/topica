namespace Topica.Contracts
{
    public interface IHandlerResolver
    {
        (object handlerImpl, object methodToValidate, object methodToExecute) ResolveHandler<T>(string source) where T: IHandler;
    }
}
using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Topica.Contracts;
using Topica.Messages;

namespace Topica.Resolvers
{
    public class HandlerResolver(IServiceProvider serviceProvider, Assembly assembly, ILogger logger) : IHandlerResolver
    {
        /// <summary>
        /// Resolves the handler for the given message type.
        /// </summary>
        /// <param name="source">The message body</param>
        /// <returns>handlerImpl, methodToValidate, methodToExecute</returns>
        /// <exception cref="Exception"></exception>
        public (bool handlerFound, object handlerImpl, object methodToValidate, object? methodToExecute) ResolveHandler(string source)
        {
            var baseMessage = JsonConvert.DeserializeObject<BaseMessage>(source);
            
            if(baseMessage == null)
            {
                logger.LogError("Message is null for: {Name}", source);
                return (false, new object(), new object(), new object());
            }
            
            if(string.IsNullOrEmpty(baseMessage.Type))
            {
                logger.LogError("Message type property is null or empty for for incoming message body: {Source}", source);
                return (false, new object(), new object(), new object());
            }
            
            var interfaceType = typeof(IHandler<>);
            var handlerTypeInterfaces = assembly.GetTypes().Where(t => t.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType));

            var handlers = handlerTypeInterfaces
                .Where(x => string.Equals(x.GetInterfaces()[0].GetGenericArguments()[0].Name, baseMessage.Type, StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            if (!handlers.Any())
            {
                logger.LogWarning("No IHandler found for incoming message type: {Name} .. Skipping message", baseMessage.Type);
                return (false, new object(), new object(), new object());
            }

            if (handlers.Count > 1)
            {
                logger.LogWarning("More than 1 IHandler found for incoming message type: {Name} .. Using last handler registered", baseMessage.Type);
            }

            var handler = handlers.Last();
            var handlersInterface = handler.GetInterfaces()[0];
            var handlerTypeParam = handlersInterface.GetGenericArguments()[0];
            var message = JsonConvert.DeserializeObject(source, handlerTypeParam);

            if (message == null)
            {
                logger.LogError("Message for {HandlerTypeParam} could not be parsed", handlerTypeParam.Name);
                return (false, new object(), new object(), new object());
            }
            
            var handlerImpl = serviceProvider.GetService(handlersInterface);

            var executeHandler = ExecuteHandler(handlerImpl, message);
            
            return (true, executeHandler.handlerImpl, executeHandler.methodToValidate, executeHandler.methodToExecute);
        }

        private static (object handlerImpl, object methodToValidate, object? methodToExecute) ExecuteHandler(object handlerImpl, object message)
        {
            var methodToValidate = handlerImpl.GetType().GetMethod("ValidateMessage");
            if (methodToValidate == null)
            {
                throw new Exception($"ValidateMessage method not found on : {handlerImpl.GetType().Name}");
            }
            var toValidate = methodToValidate.Invoke(handlerImpl, [message]);
            if (toValidate == null)
            {
                throw new Exception("invoked method returned null");   
            }
            if(!(bool)toValidate)
            {
                return (handlerImpl, toValidate, null);
            }
            
            var methodToExecute = handlerImpl.GetType().GetMethod("HandleAsync");
            if (methodToExecute == null)
            {
                throw new Exception($"HandleAsync method not found on : {handlerImpl.GetType().Name}");
            }
            var toExecute = methodToExecute.Invoke(handlerImpl, [message]);
            if (toExecute == null)
            {
                throw new Exception("invoked method returned null");   
            }

            return (handlerImpl, toValidate, toExecute);
        }
    }
}
using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Topica.Contracts;

namespace Topica.Resolvers
{
    public class HandlerResolver : IHandlerResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Assembly _assembly;

        public HandlerResolver(IServiceProvider serviceProvider, Assembly assembly)
        {
            _serviceProvider = serviceProvider;
            _assembly = assembly;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">The message body</param>
        /// <typeparam name="T">The IHandler of Base message type OR its implementation</typeparam>
        /// <returns>handlerImpl, methodToValidate, methodToExecute</returns>
        /// <exception cref="Exception"></exception>
        public (object handlerImpl, object methodToValidate, object methodToExecute) ResolveHandler<T>(string source) where T : IHandler
        {
            var interfaceType = typeof(IHandler<>);
            var handlerTypeInterfaces = _assembly.GetTypes().Where(t => t.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType));

            var handlers = handlerTypeInterfaces
                .Where(x => string.Equals(typeof(T).IsInterface ? x.GetInterfaces()[0].GetGenericArguments()[0].Name : x.Name, typeof(T).IsInterface ? typeof(T).GetGenericArguments()[0].Name : typeof(T).Name, StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            if (!handlers.Any())
            {
                throw new Exception($"No IHandler found for: {typeof(T).Name}");
            }

            if (handlers.Count > 1)
            {
                throw new Exception($"More than 1 IHandler found for: {typeof(T).Name}");
            }

            var handler = handlers.First();
            var handlersInterface = handler.GetInterfaces()[0];
            var handlerTypeParam = handlersInterface.GetGenericArguments()[0];
            var message = JsonConvert.DeserializeObject(source, handlerTypeParam);
            var handlerImpl = _serviceProvider.GetService(handlersInterface);

            return ExecuteHandler(handlerImpl, message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerType">The MessageToHandle, will find any IHandler impl that has this message type, name must match the message name</param>
        /// <param name="source">The message body</param>
        /// <returns>handlerImpl, methodToValidate, methodToExecute</returns>
        /// <exception cref="Exception"></exception>
        public (object handlerImpl, object methodToValidate, object methodToExecute) ResolveHandler(string handlerType, string source)
        {
            var interfaceType = typeof(IHandler<>);
            var handlerTypeInterfaces = _assembly.GetTypes().Where(t => t.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType));

            var handlers = handlerTypeInterfaces
                .Select(x => x.GetInterfaces()[0])
                .Where(x => string.Equals(x.GetGenericArguments()[0].Name, handlerType, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
            
            if (!handlers.Any())
            {
                throw new Exception($"No IHandler found for: {handlerType}");
            }
	
            if(handlers.Count > 1)
            {
                throw new Exception($"More than 1 IHandler found for: {handlerType}");
            }
            
            var handler = handlers.First();
            var handlerTypeParam = handler.GetGenericArguments()[0];
            var message = JsonConvert.DeserializeObject(source, handlerTypeParam);
            var handlerImpl = _serviceProvider.GetService(handler);

            return ExecuteHandler(handlerImpl, message);
        }

        private static (object handlerImpl, object methodToValidate, object methodToExecute) ExecuteHandler(object handlerImpl, object message)
        {
            var methodToValidate = handlerImpl.GetType().GetMethod("ValidateMessage");
            var methodToExecute = handlerImpl.GetType().GetMethod("HandleAsync");

            if (methodToValidate == null)
            {
                throw new Exception($"ValidateMessage method not found on : {handlerImpl.GetType().Name}");
            }
            var toValidate = methodToValidate.Invoke(handlerImpl, new[] { message });
            if (toValidate == null)
            {
                throw new Exception("invoked method returned null");   
            }
            
            if (methodToExecute == null)
            {
                throw new Exception($"HandleAsync method not found on : {handlerImpl.GetType().Name}");
            }
            var toExecute = methodToExecute.Invoke(handlerImpl, new[] { message });
            if (toExecute == null)
            {
                throw new Exception("invoked method returned null");   
            }

            return (handlerImpl, toValidate, toExecute);
        }
    }
}
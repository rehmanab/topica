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
        /// <param name="handlerType">The MessageToHandle, will find any IHandler impl that has this message type, name must match the message name</param>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public (object handlerImpl, object methodToValidate, object methodToExecute) ResolveHandler(string handlerType, string source)
        {
            var @interface = typeof(IHandler<>);
            var handlerTypeInterfaces = _assembly.GetTypes().Where(t => t.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == @interface));

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

            var methodToValidate = handlerImpl.GetType().GetMethod("ValidateMessage");
            var methodToExecute = handlerImpl.GetType().GetMethod("HandleAsync");

            if (methodToValidate == null)
            {
                throw new Exception($"ValidateMessage method not found on : {handler.Name}");
            }
            var toValidate = methodToValidate.Invoke(handlerImpl, new[] { message });
            if (toValidate == null)
            {
                throw new Exception("invoked method returned null");   
            }
            
            if (methodToExecute == null)
            {
                throw new Exception($"HandleAsync method not found on : {handler.Name}");
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
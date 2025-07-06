using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Contracts;

namespace Topica.Executors
{
    public class MessageHandlerExecutor(IHandlerResolver handlerResolver, ILoggerFactory loggerFactory) : IMessageHandlerExecutor
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger(nameof(MessageHandlerExecutor)) ?? throw new ArgumentNullException(nameof(loggerFactory));

        public async Task<(string?, bool)> ExecuteHandlerAsync(string messageBody, Dictionary<string, string>? properties)
        {
            var (handlerFound, handlerImpl, methodToValidate, methodToExecute) = handlerResolver.ResolveHandler(messageBody, properties);

            if(!handlerFound || methodToExecute == null) return (null, false);

            if (!methodToValidate)
            {
                // _logger.LogWarning("**** {Name} Validation FAILED ****", handlerImpl.GetType().Name);
                return (handlerImpl.GetType().Name, false);
            }
            
            // _logger.LogDebug("**** {Name} Execution STARTED ****", handlerImpl.GetType().Name);
            return (handlerImpl.GetType().Name, await (Task<bool>)methodToExecute);
        }
    }
}
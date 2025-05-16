using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Contracts;

namespace Topica.Executors
{
    public class MessageHandlerExecutor : IMessageHandlerExecutor
    {
        private readonly ILogger _logger;
        private readonly IHandlerResolver _handlerResolver;

        public MessageHandlerExecutor(IHandlerResolver handlerResolver, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory?.CreateLogger(nameof(MessageHandlerExecutor)) ?? throw new ArgumentNullException(nameof(loggerFactory));
            _handlerResolver = handlerResolver;
        }

        public async Task<(string, bool)> ExecuteHandlerAsync<T>(string messageBody) where T : IHandler
        {
            var (handlerImpl, methodToValidate, methodToExecute) = _handlerResolver.ResolveHandler<T>(messageBody);

            var validated = (bool)methodToValidate;
            if (!validated)
            {
                // _logger.LogWarning("**** {Name} Validation FAILED ****", handlerImpl.GetType().Name);
                return (handlerImpl.GetType().Name, false);
            }
            
            // _logger.LogDebug("**** {Name} Execution STARTED ****", handlerImpl.GetType().Name);
            return (handlerImpl.GetType().Name, await (Task<bool>)methodToExecute);
        }

        public async Task<(string, bool)> ExecuteHandlerAsync(string messageTypeName, string messageBody)
        {
            var (handlerImpl, methodToValidate, methodToExecute) = _handlerResolver.ResolveHandler(messageTypeName, messageBody);

            var validated = (bool)methodToValidate;
            if (!validated)
            {
                // _logger.LogWarning("**** {Name} Validation FAILED ****", handlerImpl.GetType().Name);
                return (handlerImpl.GetType().Name, false);
            }
            
            // _logger.LogDebug("**** {Name} Execution STARTED ****", handlerImpl.GetType().Name);
            return (handlerImpl.GetType().Name, await (Task<bool>)methodToExecute);
        }
    }
}
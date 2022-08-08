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

        public async Task<(string, bool)> ExecuteHandlerAsync(string messageTypeName, string messageBody)
        {
            var (handlerImpl, methodToValidate, methodToExecute) = _handlerResolver.ResolveHandler(messageTypeName, messageBody);

            var validated = (bool)methodToValidate;
            if (!validated)
            {
                _logger.LogInformation($"**** {handlerImpl.GetType().Name} Validation FAILED ****");
                return (handlerImpl.GetType().Name, false);
            }
            
            _logger.LogInformation($"**** {handlerImpl.GetType().Name} Execution STARTED ****");
            return (handlerImpl.GetType().Name, await (Task<bool>)methodToExecute);
        }
    }
}
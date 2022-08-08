using Aws.Consumer.Host.Messages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Topica.Contracts;

namespace Aws.Consumer.Host.Handlers
{
    public class CustomerCreatedV1MessageHandler : IHandler<CustomerCreatedV1>
    {
        private readonly ILogger<CustomerCreatedV1MessageHandler> _logger;
        
        public CustomerCreatedV1MessageHandler(ILogger<CustomerCreatedV1MessageHandler> logger)
        {
            _logger = logger;
        }

        public async Task<bool> HandleAsync(CustomerCreatedV1 source)
        {
            _logger.LogInformation(JsonConvert.SerializeObject(source));
            
            return await Task.FromResult(true);
        }

        public bool ValidateMessage(CustomerCreatedV1 message)
        {
            return true;
        }
    }
}
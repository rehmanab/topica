using Azure.ServiceBus.Queue.Consumer.Host.Settings;
using FluentValidation;

namespace Azure.ServiceBus.Queue.Consumer.Host.Validators;

public class AzureServiceBusHostSettingsValidator : AbstractValidator<AzureServiceBusHostSettings>
{
    public AzureServiceBusHostSettingsValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .WithMessage("Connection string cannot be empty.")
            .WithName("ConnectionString");
    }
}
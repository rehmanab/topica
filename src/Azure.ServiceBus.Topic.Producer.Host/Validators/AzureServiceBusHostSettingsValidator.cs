using Azure.ServiceBus.Topic.Producer.Host.Settings;
using FluentValidation;

namespace Azure.ServiceBus.Topic.Producer.Host.Validators;

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
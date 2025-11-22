using Azure.ServiceBus.Queue.Producer.Host.Settings;
using FluentValidation;

namespace Azure.ServiceBus.Queue.Producer.Host.Validators;

public class AzureServiceBusProducerSettingsValidator : AbstractValidator<AzureServiceBusProducerSettings>
{
    public AzureServiceBusProducerSettingsValidator()
    {
        RuleFor(x => x.WebAnalyticsQueueSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.WebAnalyticsQueueSettings.WorkerName).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsQueueSettings.Source).NotNull().NotEmpty();
            });
    }
}
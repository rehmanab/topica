using Azure.ServiceBus.Queue.Consumer.Host.Settings;
using FluentValidation;

namespace Azure.ServiceBus.Queue.Consumer.Host.Validators;

public class AzureServiceBusConsumerSettingsValidator : AbstractValidator<AzureServiceBusConsumerSettings>
{
    public AzureServiceBusConsumerSettingsValidator()
    {
        RuleFor(x => x.WebAnalyticsQueueSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.WebAnalyticsQueueSettings.WorkerName).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsQueueSettings.Source).NotNull().NotEmpty();
            });
    }
}
using Azure.ServiceBus.Topic.Producer.Host.Settings;
using FluentValidation;

namespace Azure.ServiceBus.Topic.Producer.Host.Validators;

public class AzureServiceBusProducerSettingsValidator : AbstractValidator<AzureServiceBusProducerSettings>
{
    public AzureServiceBusProducerSettingsValidator()
    {
        RuleFor(x => x.WebAnalyticsTopicSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.WebAnalyticsTopicSettings.WorkerName).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.Source).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.SubscribeToSource).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.Subscriptions).NotNull().NotEmpty();
            });
    }
}
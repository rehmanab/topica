using Azure.ServiceBus.Consumer.Host.Settings;
using FluentValidation;

namespace Azure.ServiceBus.Consumer.Host.Validators;

public class AzureServiceBusConsumerSettingsValidator : AbstractValidator<AzureServiceBusConsumerSettings>
{
    public AzureServiceBusConsumerSettingsValidator()
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
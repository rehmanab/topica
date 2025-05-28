using Azure.ServiceBus.Consumer.Host.Settings;
using FluentValidation;

namespace Azure.ServiceBus.Consumer.Host.Validators;

public class AzureServiceBusConsumerSettingsValidator : AbstractValidator<AzureServiceBusConsumerSettings>
{
    public AzureServiceBusConsumerSettingsValidator()
    {
        RuleFor(x => x.PriceSubmittedTopicSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.PriceSubmittedTopicSettings.Source).NotNull().NotEmpty();
                RuleFor(x => x.PriceSubmittedTopicSettings.SubscribeToSource).NotNull().NotEmpty();
                RuleFor(x => x.PriceSubmittedTopicSettings.Subscriptions).NotNull().NotEmpty();
            });


        RuleFor(x => x.QuantityUpdatedTopicSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.QuantityUpdatedTopicSettings.Source).NotNull().NotEmpty();
                RuleFor(x => x.QuantityUpdatedTopicSettings.SubscribeToSource).NotNull().NotEmpty();
                RuleFor(x => x.QuantityUpdatedTopicSettings.Subscriptions).NotNull().NotEmpty();
            });
    }
}
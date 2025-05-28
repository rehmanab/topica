using Aws.Consumer.Host.Settings;
using FluentValidation;

namespace Aws.Consumer.Host.Validators;

public class AwsConsumerSettingsValidator : AbstractValidator<AwsConsumerSettings>
{
    public AwsConsumerSettingsValidator()
    {
        RuleFor(x => x.OrderPlacedTopicSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.OrderPlacedTopicSettings.Source).NotNull().NotEmpty();
                RuleFor(x => x.OrderPlacedTopicSettings.SubscribeToSource).NotNull().NotEmpty();
                RuleFor(x => x.OrderPlacedTopicSettings.WithSubscribedQueues).NotNull().NotEmpty();
            });


        RuleFor(x => x.CustomerCreatedTopicSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.CustomerCreatedTopicSettings.Source).NotNull().NotEmpty();
                RuleFor(x => x.CustomerCreatedTopicSettings.SubscribeToSource).NotNull().NotEmpty();
                RuleFor(x => x.CustomerCreatedTopicSettings.WithSubscribedQueues).NotNull().NotEmpty();
            });
    }
}
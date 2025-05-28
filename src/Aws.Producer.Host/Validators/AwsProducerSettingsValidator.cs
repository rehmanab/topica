using Aws.Producer.Host.Settings;
using FluentValidation;

namespace Aws.Producer.Host.Validators;

public class AwsProducerSettingsValidator : AbstractValidator<AwsProducerSettings>
{
    public AwsProducerSettingsValidator()
    {
        RuleFor(x => x.OrderPlacedTopicSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.OrderPlacedTopicSettings.Source).NotNull().NotEmpty();
                RuleFor(x => x.OrderPlacedTopicSettings.SubscribeToSource).NotNull().NotEmpty();
                RuleFor(x => x.OrderPlacedTopicSettings.WithSubscribedQueues).NotNull().NotEmpty()
                    .WithMessage($"{nameof(AwsTopicSettings.WithSubscribedQueues)} cannot be null or empty.");
            });


        RuleFor(x => x.CustomerCreatedTopicSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.CustomerCreatedTopicSettings.Source).NotNull().NotEmpty();
                RuleFor(x => x.CustomerCreatedTopicSettings.SubscribeToSource).NotNull().NotEmpty();
                RuleFor(x => x.CustomerCreatedTopicSettings.WithSubscribedQueues).NotNull().NotEmpty()
                    .WithMessage($"{nameof(AwsTopicSettings.WithSubscribedQueues)} cannot be null or empty.");
            });
    }
}
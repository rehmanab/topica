using Aws.Producer.Host.Settings;
using FluentValidation;

namespace Aws.Producer.Host.Validators;

public class AwsProducerSettingsValidator : AbstractValidator<AwsProducerSettings>
{
    public AwsProducerSettingsValidator()
    {
        RuleFor(x => x.WebAnalyticsTopicSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.WebAnalyticsTopicSettings.WorkerName).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.Source).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.SubscribeToSource).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.WithSubscribedQueues).NotNull().NotEmpty()
                    .WithMessage($"{nameof(AwsTopicSettings.WithSubscribedQueues)} cannot be null or empty.");
            });
    }
}
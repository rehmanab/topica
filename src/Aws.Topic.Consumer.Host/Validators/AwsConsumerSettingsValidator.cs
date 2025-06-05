using Aws.Topic.Consumer.Host.Settings;
using FluentValidation;

namespace Aws.Topic.Consumer.Host.Validators;

public class AwsConsumerSettingsValidator : AbstractValidator<AwsConsumerSettings>
{
    public AwsConsumerSettingsValidator()
    {
        RuleFor(x => x.WebAnalyticsTopicSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.WebAnalyticsTopicSettings.WorkerName).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.Source).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.SubscribeToSource).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.WithSubscribedQueues).NotNull().NotEmpty();
            });
    }
}
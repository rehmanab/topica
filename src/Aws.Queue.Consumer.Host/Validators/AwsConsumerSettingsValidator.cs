using Aws.Queue.Consumer.Host.Settings;
using FluentValidation;

namespace Aws.Queue.Consumer.Host.Validators;

public class AwsConsumerSettingsValidator : AbstractValidator<AwsConsumerSettings>
{
    public AwsConsumerSettingsValidator()
    {
        RuleFor(x => x.WebAnalyticsQueueSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.WebAnalyticsQueueSettings.WorkerName).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsQueueSettings.Source).NotNull().NotEmpty();
            });
    }
}
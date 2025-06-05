using Aws.Queue.Producer.Host.Settings;
using FluentValidation;

namespace Aws.Queue.Producer.Host.Validators;

public class AwsProducerSettingsValidator : AbstractValidator<AwsProducerSettings>
{
    public AwsProducerSettingsValidator()
    {
        RuleFor(x => x.WebAnalyticsQueueSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.WebAnalyticsQueueSettings.WorkerName).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsQueueSettings.Source).NotNull().NotEmpty();
            });
    }
}
using FluentValidation;
using Pulsar.Producer.Host.Settings;

namespace Pulsar.Producer.Host.Validators;

public class PulsarProducerSettingsValidator : AbstractValidator<PulsarProducerSettings>
{
    public PulsarProducerSettingsValidator()
    {
        RuleFor(x => x.WebAnalyticsTopicSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.WebAnalyticsTopicSettings.WorkerName).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.Source).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.Tenant).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.Namespace).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.ConsumerGroup).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.StartNewConsumerEarliest).NotNull().NotEmpty();
            });
    }
}
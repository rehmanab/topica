using FluentValidation;
using Pulsar.Topic.Consumer.Host.Settings;

namespace Pulsar.Topic.Consumer.Host.Validators;

public class PulsarConsumerSettingsValidator : AbstractValidator<PulsarConsumerSettings>
{
    public PulsarConsumerSettingsValidator()
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
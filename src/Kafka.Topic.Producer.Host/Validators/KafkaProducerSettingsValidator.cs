using FluentValidation;
using Kafka.Topic.Producer.Host.Settings;

namespace Kafka.Topic.Producer.Host.Validators;

public class KafkaProducerSettingsValidator : AbstractValidator<KafkaProducerSettings>
{
    public KafkaProducerSettingsValidator()
    {
        RuleFor(x => x.WebAnalyticsTopicSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.WebAnalyticsTopicSettings.WorkerName).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.Source).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.ConsumerGroup).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.StartFromEarliestMessages).NotNull().NotEmpty();
            });
    }
}
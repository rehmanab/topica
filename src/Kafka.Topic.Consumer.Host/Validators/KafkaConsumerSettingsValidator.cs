using FluentValidation;
using Kafka.Topic.Consumer.Host.Settings;

namespace Kafka.Topic.Consumer.Host.Validators;

public class KafkaConsumerSettingsValidator : AbstractValidator<KafkaConsumerSettings>
{
    public KafkaConsumerSettingsValidator()
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
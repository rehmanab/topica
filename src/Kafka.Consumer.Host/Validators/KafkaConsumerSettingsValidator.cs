using FluentValidation;
using Kafka.Consumer.Host.Settings;

namespace Kafka.Consumer.Host.Validators;

public class KafkaConsumerSettingsValidator : AbstractValidator<KafkaConsumerSettings>
{
    public KafkaConsumerSettingsValidator()
    {
        RuleFor(x => x.PersonCreatedTopicSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.PersonCreatedTopicSettings.Source).NotNull().NotEmpty();
                RuleFor(x => x.PersonCreatedTopicSettings.ConsumerGroup).NotNull().NotEmpty();
                RuleFor(x => x.PersonCreatedTopicSettings.StartFromEarliestMessages).NotNull().NotEmpty();
            });


        RuleFor(x => x.PlaceCreatedTopicSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.PlaceCreatedTopicSettings.Source).NotNull().NotEmpty();
                RuleFor(x => x.PlaceCreatedTopicSettings.ConsumerGroup).NotNull().NotEmpty();
                RuleFor(x => x.PlaceCreatedTopicSettings.StartFromEarliestMessages).NotNull().NotEmpty();
            });
    }
}
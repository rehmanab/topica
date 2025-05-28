using FluentValidation;
using Kafka.Producer.Host.Settings;

namespace Kafka.Producer.Host.Validators;

public class KafkaProducerSettingsValidator : AbstractValidator<KafkaProducerSettings>
{
    public KafkaProducerSettingsValidator()
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
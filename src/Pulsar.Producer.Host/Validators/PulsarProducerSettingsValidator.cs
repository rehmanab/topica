using FluentValidation;
using Pulsar.Producer.Host.Settings;

namespace Pulsar.Producer.Host.Validators;

public class PulsarProducerSettingsValidator : AbstractValidator<PulsarProducerSettings>
{
    public PulsarProducerSettingsValidator()
    {
        RuleFor(x => x.DataSentTopicSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.DataSentTopicSettings.Source).NotNull().NotEmpty();
                RuleFor(x => x.DataSentTopicSettings.Tenant).NotNull().NotEmpty();
                RuleFor(x => x.DataSentTopicSettings.Namespace).NotNull().NotEmpty();
                RuleFor(x => x.DataSentTopicSettings.ConsumerGroup).NotNull().NotEmpty();
                RuleFor(x => x.DataSentTopicSettings.StartNewConsumerEarliest).NotNull().NotEmpty();
            });


        RuleFor(x => x.MatchStartedTopicSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.MatchStartedTopicSettings.Source).NotNull().NotEmpty();
                RuleFor(x => x.MatchStartedTopicSettings.Tenant).NotNull().NotEmpty();
                RuleFor(x => x.MatchStartedTopicSettings.Namespace).NotNull().NotEmpty();
                RuleFor(x => x.MatchStartedTopicSettings.ConsumerGroup).NotNull().NotEmpty();
                RuleFor(x => x.MatchStartedTopicSettings.StartNewConsumerEarliest).NotNull().NotEmpty();
            });
    }
}
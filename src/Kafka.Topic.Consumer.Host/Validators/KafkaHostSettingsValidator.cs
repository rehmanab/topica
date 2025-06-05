using FluentValidation;
using Kafka.Topic.Consumer.Host.Settings;

namespace Kafka.Topic.Consumer.Host.Validators;

public class KafkaHostSettingsValidator : AbstractValidator<KafkaHostSettings>
{
    public KafkaHostSettingsValidator()
    {
        RuleFor(x => x.BootstrapServers)
            .NotNull()
            .NotEmpty();
    }
}
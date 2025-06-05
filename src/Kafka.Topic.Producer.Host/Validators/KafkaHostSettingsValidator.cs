using FluentValidation;
using Kafka.Topic.Producer.Host.Settings;

namespace Kafka.Topic.Producer.Host.Validators;

public class KafkaHostSettingsValidator : AbstractValidator<KafkaHostSettings>
{
    public KafkaHostSettingsValidator()
    {
        RuleFor(x => x.BootstrapServers)
            .NotNull()
            .NotEmpty();
    }
}
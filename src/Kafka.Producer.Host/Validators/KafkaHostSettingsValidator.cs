using FluentValidation;
using Kafka.Producer.Host.Settings;

namespace Kafka.Producer.Host.Validators;

public class KafkaHostSettingsValidator : AbstractValidator<KafkaHostSettings>
{
    public KafkaHostSettingsValidator()
    {
        RuleFor(x => x.BootstrapServers)
            .NotNull()
            .NotEmpty();
    }
}
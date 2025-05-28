using FluentValidation;
using Kafka.Consumer.Host.Settings;

namespace Kafka.Consumer.Host.Validators;

public class KafkaHostSettingsValidator : AbstractValidator<KafkaHostSettings>
{
    public KafkaHostSettingsValidator()
    {
        RuleFor(x => x.BootstrapServers)
            .NotNull()
            .NotEmpty();
    }
}
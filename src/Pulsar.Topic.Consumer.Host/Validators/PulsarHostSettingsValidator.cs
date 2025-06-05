using FluentValidation;
using Pulsar.Topic.Consumer.Host.Settings;

namespace Pulsar.Topic.Consumer.Host.Validators;

public class PulsarHostSettingsValidator : AbstractValidator<PulsarHostSettings>
{
    public PulsarHostSettingsValidator()
    {
        RuleFor(x => x.ServiceUrl)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.PulsarManagerBaseUrl)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.PulsarAdminBaseUrl)
            .NotNull()
            .NotEmpty();
    }
}
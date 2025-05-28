using FluentValidation;
using Pulsar.Consumer.Host.Settings;

namespace Pulsar.Consumer.Host.Validators;

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
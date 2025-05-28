using FluentValidation;
using Pulsar.Producer.Host.Settings;

namespace Pulsar.Producer.Host.Validators;

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
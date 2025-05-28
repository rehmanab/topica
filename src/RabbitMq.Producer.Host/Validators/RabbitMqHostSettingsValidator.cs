using FluentValidation;
using RabbitMq.Producer.Host.Settings;

namespace RabbitMq.Producer.Host.Validators;

public class RabbitMqHostSettingsValidator : AbstractValidator<RabbitMqHostSettings>
{
    public RabbitMqHostSettingsValidator()
    {
        RuleFor(x => x.Scheme).NotNull().NotEmpty();
        RuleFor(x => x.Hostname).NotNull().NotEmpty();
        RuleFor(x => x.UserName).NotNull().NotEmpty();
        RuleFor(x => x.Password).NotNull().NotEmpty();
        RuleFor(x => x.Port).NotNull().NotEmpty();
        RuleFor(x => x.VHost).NotNull().NotEmpty();
        RuleFor(x => x.ManagementPort).NotNull().NotEmpty();
        RuleFor(x => x.ManagementScheme).NotNull().NotEmpty();
    }
}
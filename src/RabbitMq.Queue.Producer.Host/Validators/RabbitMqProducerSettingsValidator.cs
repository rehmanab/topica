using FluentValidation;
using RabbitMq.Queue.Producer.Host.Settings;

namespace RabbitMq.Queue.Producer.Host.Validators;

public class RabbitMqProducerSettingsValidator : AbstractValidator<RabbitMqProducerSettings>
{
    public RabbitMqProducerSettingsValidator()
    {
        RuleFor(x => x.WebAnalyticsTopicSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.WebAnalyticsTopicSettings.WorkerName).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.Source).NotNull().NotEmpty();
            });
    }
}
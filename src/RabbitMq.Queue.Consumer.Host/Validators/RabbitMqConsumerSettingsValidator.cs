using FluentValidation;
using RabbitMq.Queue.Consumer.Host.Settings;

namespace RabbitMq.Queue.Consumer.Host.Validators;

public class RabbitMqConsumerSettingsValidator : AbstractValidator<RabbitMqConsumerSettings>
{
    public RabbitMqConsumerSettingsValidator()
    {
        RuleFor(x => x.WebAnalyticsQueueSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.WebAnalyticsQueueSettings.WorkerName).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsQueueSettings.Source).NotNull().NotEmpty();
            });
    }
}
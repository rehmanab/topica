using FluentValidation;
using RabbitMq.Queue.Consumer.Host.Settings;

namespace RabbitMq.Queue.Consumer.Host.Validators;

public class RabbitMqConsumerSettingsValidator : AbstractValidator<RabbitMqConsumerSettings>
{
    public RabbitMqConsumerSettingsValidator()
    {
        RuleFor(x => x.WebAnalyticsTopicSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.WebAnalyticsTopicSettings.WorkerName).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.Source).NotNull().NotEmpty();
            });
    }
}
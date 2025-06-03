using FluentValidation;
using RabbitMq.Consumer.Host.Settings;

namespace RabbitMq.Consumer.Host.Validators;

public class RabbitMqConsumerSettingsValidator : AbstractValidator<RabbitMqConsumerSettings>
{
    public RabbitMqConsumerSettingsValidator()
    {
        RuleFor(x => x.WebAnalyticsTopicSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.WebAnalyticsTopicSettings.WorkerName).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.Source).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.WithSubscribedQueues).NotNull().NotEmpty();
                RuleFor(x => x.WebAnalyticsTopicSettings.SubscribeToSource).NotNull().NotEmpty();
            });
    }
}
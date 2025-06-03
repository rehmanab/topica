using FluentValidation;
using RabbitMq.Producer.Host.Settings;

namespace RabbitMq.Producer.Host.Validators;

public class RabbitMqProducerSettingsValidator : AbstractValidator<RabbitMqProducerSettings>
{
    public RabbitMqProducerSettingsValidator()
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
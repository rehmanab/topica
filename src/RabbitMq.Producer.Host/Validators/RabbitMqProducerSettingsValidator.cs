using FluentValidation;
using RabbitMq.Producer.Host.Settings;

namespace RabbitMq.Producer.Host.Validators;

public class RabbitMqProducerSettingsValidator : AbstractValidator<RabbitMqProducerSettings>
{
    public RabbitMqProducerSettingsValidator()
    {
        RuleFor(x => x.ItemDeliveredTopicSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.ItemDeliveredTopicSettings.Source).NotNull().NotEmpty();
                RuleFor(x => x.ItemDeliveredTopicSettings.WithSubscribedQueues).NotNull().NotEmpty();
                RuleFor(x => x.ItemDeliveredTopicSettings.SubscribeToSource).NotNull().NotEmpty();
            });
        
        RuleFor(x => x.ItemPostedTopicSettings)
            .NotNull().DependentRules(() =>
            {
                RuleFor(x => x.ItemPostedTopicSettings.Source).NotNull().NotEmpty();
                RuleFor(x => x.ItemPostedTopicSettings.WithSubscribedQueues).NotNull().NotEmpty();
                RuleFor(x => x.ItemPostedTopicSettings.SubscribeToSource).NotNull().NotEmpty();
            });
    }
}
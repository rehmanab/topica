using Aws.Queue.Producer.Host.Settings;
using FluentValidation;

namespace Aws.Queue.Producer.Host.Validators;

public class AwsHostSettingsValidator : AbstractValidator<AwsHostSettings>
{
    public AwsHostSettingsValidator()
    {
        RuleFor(x => x)
            .Must(x =>
            {
                var profileNameIsEmpty = string.IsNullOrWhiteSpace(x.ProfileName);
                var accessKeyIdIsEmpty = string.IsNullOrWhiteSpace(x.AccessKey);
                var secretAccessKeyIsEmpty = string.IsNullOrWhiteSpace(x.SecretKey);
                var serviceUrlKeyIsEmpty = string.IsNullOrWhiteSpace(x.ServiceUrl);

                switch (profileNameIsEmpty)
                {
                    case true when accessKeyIdIsEmpty && secretAccessKeyIsEmpty && !serviceUrlKeyIsEmpty:
                        return true;
                    case true when accessKeyIdIsEmpty && secretAccessKeyIsEmpty:
                    case true when accessKeyIdIsEmpty:
                    case true when secretAccessKeyIsEmpty:
                        return false; // At least one of the fields must be provided
                    default:
                        return true; // Valid configuration
                }
            })
            .WithMessage($"{nameof(AwsHostSettings)} app setting: At least one of the following must be provided: ProfileName, (AccessKey & SecretKey) or just the ServiceUrl. If ProfileName is not provided, AccessKey and SecretKey must be provided.");
    }
}
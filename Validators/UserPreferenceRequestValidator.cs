using FluentValidation;
using weatherapp.Requests;

namespace weatherapp.Validators
{
    public class UserPreferenceRequestValidator : AbstractValidator<UserPreferenceRequest>
    {
        public UserPreferenceRequestValidator()
        {
            RuleFor(x => x.RefreshInterval)
                .InclusiveBetween(1, 1440).WithMessage("Refresh interval must be between 1 and 1440 minutes (1 day).")
                .When(x => x.RefreshInterval.HasValue);
        }
    }
}
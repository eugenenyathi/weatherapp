using FluentValidation;
using weatherapp.Requests;

namespace weatherapp.Validators;

public class UpdateTrackLocationRequestValidator : AbstractValidator<UpdateTrackLocationRequest>
{
	public UpdateTrackLocationRequestValidator()
	{
		RuleFor(x => x.DisplayName)
			.MaximumLength(30).WithMessage("Display name cannot exceed 30 characters.")
			.When(x => !string.IsNullOrEmpty(x.DisplayName));
	}
}
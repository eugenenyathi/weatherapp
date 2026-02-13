using FluentValidation;
using weatherapp.Requests;

namespace weatherapp.Validators;

public class TrackLocationRequestValidator : AbstractValidator<TrackLocationRequest>
{
	public TrackLocationRequestValidator()
	{
		RuleFor(x => x.LocationId)
			.NotEmpty().WithMessage("Location ID is required.");

		RuleFor(x => x.DisplayName)
			.MaximumLength(30).WithMessage("Display name cannot exceed 30 characters.")
			.When(x => !string.IsNullOrEmpty(x.DisplayName));
	}
}
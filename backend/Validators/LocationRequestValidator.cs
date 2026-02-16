using FluentValidation;
using weatherapp.Requests;

namespace weatherapp.Validators;

public class LocationRequestValidator : AbstractValidator<LocationRequest>
{
	public LocationRequestValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Location name is required.")
			.MaximumLength(200).WithMessage("Location name cannot exceed 200 characters.");

		RuleFor(x => x.Latitude)
			.InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90 degrees.");

		RuleFor(x => x.Longitude)
			.InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180 degrees.");

		RuleFor(x => x.Country)
			.NotEmpty().WithMessage("Country is required.")
			.MaximumLength(100).WithMessage("Country name cannot exceed 100 characters.");
	}
}
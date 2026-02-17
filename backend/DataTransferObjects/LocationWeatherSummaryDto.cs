using weatherapp.Enums;

namespace weatherapp.DataTransferObjects;

public class LocationWeatherSummaryDto
{
	public Guid Id { get; set; }
	public Guid LocationId { get; set; }
	public string LocationName { get; set; }
	public DateOnly Date { get; set; }
	public decimal MinTemp { get; set; }
	public decimal MaxTemp { get; set; }
	public decimal Rain { get; set; }
	public Unit Unit { get; set; }
	public bool isFavorite { get; set; }
}
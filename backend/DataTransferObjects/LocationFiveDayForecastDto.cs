using weatherapp.Enums;

namespace weatherapp.DataTransferObjects;

public class LocationFiveDayForecastDto
{
	public Guid Id { get; set; }
	public Guid LocationId { get; set; }
	public string LocationName { get; set; }
	public Unit Unit { get; set; }
	public List<DayWeatherDto> FiveDayForecasts { get; set; }
}
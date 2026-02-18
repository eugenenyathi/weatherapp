using weatherapp.Enums;

namespace weatherapp.DataTransferObjects;

public class LocationHourlyForecastDto
{
	public Guid LocationId { get; set; }
	public string LocationName { get; set; } = string.Empty;
	public Unit Unit { get; set; }
	public List<HourWeatherDto> HourlyForecasts { get; set; } = new();
	public DateTime LastSyncedAt { get; set; }
}

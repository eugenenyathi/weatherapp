using weatherapp.DataTransferObjects;

namespace weatherapp.Services.Interfaces;

public interface IWeatherForecastService
{
	Task<List<LocationWeatherSummaryDto>> GetCurrentDaySummariesForAllTrackedLocationsAsync(Guid userId);
	Task<LocationFiveDayForecastDto> GetFiveDayForecastForLocationAsync(Guid locationId, Guid userId);
}
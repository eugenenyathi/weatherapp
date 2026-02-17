using weatherapp.Entities;
using weatherapp.Enums;
using weatherapp.DataTransferObjects;

namespace weatherapp.Services.Interfaces;

public interface IOpenWeatherService
{
	Task GetLocationHourlyWeather(Location location);
	Task GetLocationDailyWeather(Location location);
	Task SyncLocationsDailyWeather();
	Task SyncWeatherForUserTrackedLocationsAsync(Guid userId);
}
using weatherapp.Entities;

namespace weatherapp.Services.Interfaces;

public interface IOpenWeatherService
{
	Task GetLocationHourlyWeather(Location location);
	Task GetLocationDailyWeather(Location location);
	Task SyncLocationsDailyWeather();
}
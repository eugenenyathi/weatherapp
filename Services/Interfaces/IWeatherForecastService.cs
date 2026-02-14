using weatherapp.DataTransferObjects;

namespace weatherapp.Services.Interfaces;

public interface IWeatherForecastService
{
	Task<List<DailyWeatherDto>> GetTrackedForecastsAsync(Guid userId);
}
using Microsoft.EntityFrameworkCore;
using weatherapp.Data;
using weatherapp.DataTransferObjects;
using weatherapp.Enums;

namespace weatherapp.Services;

public class WeatherForecastService(AppDbContext context)
{
	public async Task<List<DailyWeatherDto>> GetTrackedForecastsAsync(Guid userId)
	{
		// 1. Fetch User Preferences for Unit selection
		var preference = await context.UserPreferences
			.FirstOrDefaultAsync(p => p.UserId == userId);

		var unit = preference?.PreferredUnit ?? Unit.Metric; // Default to Metric
		var today = DateOnly.FromDateTime(DateTime.UtcNow);

		// 2. Fetch Tracked Locations including their future forecasts
		var trackedLocations = await context.TrackLocations
			.Include(tl => tl.Location)
			.ThenInclude(l => l.DailyWeathers.Where(dw => dw.Date >= today).OrderBy(dw => dw.Date))
			.Where(tl => tl.UserId == userId)
			.ToListAsync();

		// 3. Map to DTOs
		return trackedLocations.Select(tl => new DailyWeatherDto
		{
			Location = tl.DisplayName ?? tl.Location.Name,
			Unit = unit,
			DayWeathers = tl.Location.DailyWeathers.Select(dw => new DayWeatherDto
			{
				Date = dw.Date,
				Humidity = (int)dw.Humidity,
				// Conditional mapping based on preference
				MinTemp = unit == Unit.Metric ? dw.MinTempMetric : dw.MinTempImperial,
				MaxTemp = unit == Unit.Metric ? dw.MaxTempMetric : dw.MaxTempImperial
			}).ToList()
		}).ToList();
	}
}
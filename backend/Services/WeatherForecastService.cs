using Microsoft.EntityFrameworkCore;
using weatherapp.Data;
using weatherapp.DataTransferObjects;
using weatherapp.Enums;
using weatherapp.Services.Interfaces;

namespace weatherapp.Services;

public class WeatherForecastService(AppDbContext context): IWeatherForecastService
{

	public async Task<List<LocationWeatherSummaryDto>> GetCurrentDaySummariesForAllTrackedLocationsAsync(Guid userId)
	{
		var preference = await context.UserPreferences
			.FirstOrDefaultAsync(p => p.UserId == userId);

		var unit = preference?.PreferredUnit ?? Unit.Metric; // Default to Metric
		var today = DateOnly.FromDateTime(DateTime.UtcNow);

		// Fetch tracked locations with their current day weather
		var trackedLocations = await context.TrackLocations
			.Include(tl => tl.Location)
			.ThenInclude(l => l.DailyWeathers.Where(dw => dw.Date == today))
			.Where(tl => tl.UserId == userId)
			.ToListAsync();

		// Map to DTOs with current day summaries
		return trackedLocations.Select(tl => 
		{
			var currentDayWeather = tl.Location.DailyWeathers.FirstOrDefault(dw => dw.Date == today);
			
			return new LocationWeatherSummaryDto
			{
				LocationId = tl.LocationId,
				LocationName = tl.DisplayName ?? tl.Location.Name,
				Date = today,
				MinTemp = currentDayWeather != null 
					? (unit == Unit.Metric ? currentDayWeather.MinTempMetric : currentDayWeather.MinTempImperial) 
					: 0,
				MaxTemp = currentDayWeather != null 
					? (unit == Unit.Metric ? currentDayWeather.MaxTempMetric : currentDayWeather.MaxTempImperial) 
					: 0,
				Rain = currentDayWeather?.Rain ?? 0,
				Unit = unit
			};
		}).ToList();
	}

	public async Task<LocationFiveDayForecastDto> GetFiveDayForecastForLocationAsync(Guid locationId, Guid userId)
	{
		var preference = await context.UserPreferences
			.FirstOrDefaultAsync(p => p.UserId == userId);

		var unit = preference?.PreferredUnit ?? Unit.Metric; // Default to Metric
		var today = DateOnly.FromDateTime(DateTime.UtcNow);

		// Check if the user is tracking this location
		var isTracking = await context.TrackLocations
			.AnyAsync(tl => tl.LocationId == locationId && tl.UserId == userId);

		if (!isTracking)
		{
			throw new UnauthorizedAccessException("User is not authorized to access this location's forecast.");
		}

		// Fetch the location with its 5-day forecast (including current day)
		var location = await context.Locations
			.Where(l => l.Id == locationId)
			.Include(l => l.DailyWeathers
				.Where(dw => dw.Date >= today && dw.Date <= today.AddDays(4))
				.OrderBy(dw => dw.Date))
			.FirstOrDefaultAsync();

		if (location == null)
		{
			throw new ArgumentException("Location not found.");
		}

		return new LocationFiveDayForecastDto
		{
			LocationId = location.Id,
			LocationName = location.Name,
			Unit = unit,
			FiveDayForecasts = location.DailyWeathers.Select(dw => new DayWeatherDto
			{
				Date = dw.Date,
				MinTemp = unit == Unit.Metric ? dw.MinTempMetric : dw.MinTempImperial,
				MaxTemp = unit == Unit.Metric ? dw.MaxTempMetric : dw.MaxTempImperial,
				Rain = dw.Rain ?? 0,
				Humidity = (int)dw.Humidity,
				Summary = dw.Summary
			}).ToList()
		};
	}
}
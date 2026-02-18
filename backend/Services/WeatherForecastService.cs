using Microsoft.EntityFrameworkCore;
using weatherapp.Data;
using weatherapp.DataTransferObjects;
using weatherapp.Enums;
using weatherapp.Services.Interfaces;

namespace weatherapp.Services;

public class WeatherForecastService(AppDbContext context, ILocationService locationService) : IWeatherForecastService
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
			.Include(tl => tl.Location.LocationJobs.Where(lj => lj.Status == "Pending" || lj.Status == "Processing"))
			.Where(tl => tl.UserId == userId)
			.ToListAsync();

		// Wait for any pending background jobs for these locations
		var pendingJobLocations = trackedLocations
			.Where(tl => tl.Location.LocationJobs.Any(lj => lj.Status == "Pending" || lj.Status == "Processing"))
			.Select(tl => tl.LocationId)
			.Distinct()
			.ToList();

		foreach (var locationId in pendingJobLocations)
		{
			await locationService.WaitForLocationWeatherDataAsync(locationId);
		}

		// Re-fetch to get updated weather data after jobs complete
		trackedLocations = await context.TrackLocations
			.Include(tl => tl.Location)
				.ThenInclude(l => l.DailyWeathers.Where(dw => dw.Date == today))
			.Where(tl => tl.UserId == userId)
			.ToListAsync();

		// Get the last synced time for the user's locations
		var lastSyncedAt = await context.LocationSyncSchedules
			.Where(lss => lss.UserId == userId)
			.MaxAsync(lss => (DateTime?)lss.LastSyncAt) ?? DateTime.MinValue;

		// Map to DTOs with current day summaries
		return trackedLocations.Select(tl =>
		{
			var currentDayWeather = tl.Location.DailyWeathers.FirstOrDefault(dw => dw.Date == today);

			return new LocationWeatherSummaryDto
			{
				Id = tl.Id,
				LocationId = tl.LocationId,
				LocationName = tl.DisplayName ?? tl.Location.Name,
				Date = today,
				isFavorite = tl.isFavorite,
				MinTemp = currentDayWeather != null
					? (unit == Unit.Metric ? currentDayWeather.MinTempMetric : currentDayWeather.MinTempImperial)
					: 0,
				MaxTemp = currentDayWeather != null
					? (unit == Unit.Metric ? currentDayWeather.MaxTempMetric : currentDayWeather.MaxTempImperial)
					: 0,
				Rain = currentDayWeather?.Rain ?? 0,
				Unit = unit,
				LastSyncedAt = lastSyncedAt
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

		// Check if there's a pending location job and wait for it
		var hasPendingLocationJob = await context.LocationJobs
			.AnyAsync(lj => lj.LocationId == locationId && (lj.Status == "Pending" || lj.Status == "Processing"));

		if (hasPendingLocationJob)
		{
			await locationService.WaitForLocationWeatherDataAsync(locationId);
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

		// Get the last synced time for this user-location pair
		var lastSyncedAt = await context.LocationSyncSchedules
			.Where(lss => lss.UserId == userId && lss.LocationId == locationId)
			.MaxAsync(lss => (DateTime?)lss.LastSyncAt) ?? DateTime.MinValue;

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
			}).ToList(),
			LastSyncedAt = lastSyncedAt
		};
	}

	public async Task<LocationHourlyForecastDto> GetHourlyForecastForLocationAsync(Guid locationId, Guid userId)
	{
		var preference = await context.UserPreferences
			.FirstOrDefaultAsync(p => p.UserId == userId);

		var unit = preference?.PreferredUnit ?? Unit.Metric; // Default to Metric
		var now = DateTime.UtcNow;

		// Check if the user is tracking this location
		var isTracking = await context.TrackLocations
			.AnyAsync(tl => tl.LocationId == locationId && tl.UserId == userId);

		if (!isTracking)
		{
			throw new UnauthorizedAccessException("User is not authorized to access this location's forecast.");
		}

		// Check if there's a pending location job and wait for it
		var hasPendingLocationJob = await context.LocationJobs
			.AnyAsync(lj => lj.LocationId == locationId && (lj.Status == "Pending" || lj.Status == "Processing"));

		if (hasPendingLocationJob)
		{
			await locationService.WaitForLocationWeatherDataAsync(locationId);
		}

		// Fetch the location with its hourly forecast (next 24 hours)
		var location = await context.Locations
			.Where(l => l.Id == locationId)
			.Include(l => l.HourlyWeathers
				.Where(hw => hw.DateTime >= now && hw.DateTime <= now.AddHours(24))
				.OrderBy(hw => hw.DateTime))
			.FirstOrDefaultAsync();

		if (location == null)
		{
			throw new ArgumentException("Location not found.");
		}

		// Get the last synced time for this user-location pair
		var lastSyncedAt = await context.LocationSyncSchedules
			.Where(lss => lss.UserId == userId && lss.LocationId == locationId)
			.MaxAsync(lss => (DateTime?)lss.LastSyncAt) ?? DateTime.MinValue;

		return new LocationHourlyForecastDto
		{
			LocationId = location.Id,
			LocationName = location.Name,
			Unit = unit,
			HourlyForecasts = location.HourlyWeathers.Select(hw => new HourWeatherDto
			{
				DateTime = hw.DateTime,
				TempMetric = hw.TempMetric,
				TempImperial = hw.TempImperial,
				Humidity = hw.Humidity
			}).ToList(),
			LastSyncedAt = lastSyncedAt
		};
	}
}

using Hangfire;
using Microsoft.EntityFrameworkCore;
using weatherapp.Data;
using weatherapp.Entities;
using weatherapp.Services.Interfaces;

namespace weatherapp.Services;

public class SyncScheduleService(
	AppDbContext context,
	IRecurringJobManager recurringJobManager,
	IOpenWeatherService openWeatherService) : ISyncScheduleService
{
	public async Task InitializeSyncSchedulesForUserAsync(Guid userId, int refreshIntervalMinutes)
	{
		// Get all locations the user is tracking
		var trackedLocations = await context.TrackLocations
			.Where(tl => tl.UserId == userId)
			.Select(tl => tl.LocationId)
			.ToListAsync();

		if (!trackedLocations.Any()) return;

		// Create sync schedules for each location
		foreach (var locationId in trackedLocations)
		{
			var recurringJobId = $"user-{userId}-location-{locationId}-sync";
			var cronExpression = GetCronFromMinutes(refreshIntervalMinutes);

			// Register recurring job
			recurringJobManager.AddOrUpdate(
				recurringJobId,
				() => openWeatherService.SyncWeatherForUserTrackedLocationsAsync(userId),
				cronExpression
			);

			// Create sync schedule record
			var syncSchedule = new LocationSyncSchedule
			{
				UserId = userId,
				LocationId = locationId,
				RecurringJobId = recurringJobId,
				LastSyncAt = DateTime.MinValue,
				NextSyncAt = DateTime.UtcNow.AddMinutes(refreshIntervalMinutes)
			};

			context.LocationSyncSchedules.Add(syncSchedule);
		}

		await context.SaveChangesAsync();
	}

	public async Task UpdateSyncSchedulesForUserAsync(Guid userId, int newRefreshIntervalMinutes)
	{
		// Get all sync schedules for this user
		var syncSchedules = await context.LocationSyncSchedules
			.Where(lss => lss.UserId == userId)
			.ToListAsync();

		if (!syncSchedules.Any())
		{
			// No existing schedules, initialize new ones
			await InitializeSyncSchedulesForUserAsync(userId, newRefreshIntervalMinutes);
			return;
		}

		var cronExpression = GetCronFromMinutes(newRefreshIntervalMinutes);

		// Update each recurring job with new interval
		foreach (var schedule in syncSchedules)
		{
			recurringJobManager.AddOrUpdate(
				schedule.RecurringJobId,
				() => openWeatherService.SyncWeatherForUserTrackedLocationsAsync(userId),
				cronExpression
			);

			// Update next sync time
			schedule.NextSyncAt = DateTime.UtcNow.AddMinutes(newRefreshIntervalMinutes);
		}

		await context.SaveChangesAsync();
	}

	public async Task RemoveSyncSchedulesForUserAsync(Guid userId)
	{
		// Get all sync schedules for this user
		var syncSchedules = await context.LocationSyncSchedules
			.Where(lss => lss.UserId == userId)
			.ToListAsync();

		if (!syncSchedules.Any()) return;

		// Remove all recurring jobs
		foreach (var schedule in syncSchedules)
		{
			recurringJobManager.RemoveIfExists(schedule.RecurringJobId);
		}

		// Remove sync schedule records
		context.LocationSyncSchedules.RemoveRange(syncSchedules);
		await context.SaveChangesAsync();
	}

	public async Task TriggerSyncForUserAsync(Guid userId)
	{
		// Trigger immediate sync via fire-and-forget job
		BackgroundJob.Enqueue(() => openWeatherService.SyncWeatherForUserTrackedLocationsAsync(userId));
	}

	public async Task InitializeAllUserSyncSchedulesAsync()
	{
		// Get all user preferences from the database
		var userPreferences = await context.UserPreferences.ToListAsync();

		foreach (var preference in userPreferences)
		{
			// Re-register recurring jobs for each user
			await UpdateSyncSchedulesForUserAsync(preference.UserId, preference.RefreshInterval);
		}
	}

	private string GetCronFromMinutes(int minutes)
	{
		// Convert minutes to cron expression
		// Hangfire cron: minute hour day month dayOfWeek

		if (minutes <= 0) minutes = 30; // Default to 30 minutes

		// For intervals less than 60 minutes, 
		if (minutes < 60)
		{
			return $"*/{minutes} * * * *";
		}

		// For hourly intervals
		if (minutes % 60 == 0)
		{
			var hours = minutes / 60;
			return $"0 */{hours} * * *";
		}

		// For other intervals, approximate to nearest hour
		var wholeHours = minutes / 60;
		return $"0 */{wholeHours} * * *";
	}
}

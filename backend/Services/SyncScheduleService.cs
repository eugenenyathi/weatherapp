using Hangfire;
using Microsoft.EntityFrameworkCore;
using weatherapp.Data;
using weatherapp.DataTransferObjects;
using weatherapp.Entities;
using weatherapp.Exceptions;
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
			// Use shorter job ID format to avoid SQL truncation
			var recurringJobId = $"sync:{userId.ToString("N").Substring(0, 8)}:{locationId.ToString("N").Substring(0, 8)}";
			var cronExpression = GetCronFromMinutes(refreshIntervalMinutes);

			// Register recurring job
			recurringJobManager.AddOrUpdate(
				recurringJobId,
				() => openWeatherService.SyncWeatherForUserTrackedLocationsAsync(userId),
				cronExpression
			);

			// Create or update sync schedule record
			var syncSchedule = await context.LocationSyncSchedules
				.FirstOrDefaultAsync(lss => lss.UserId == userId && lss.LocationId == locationId);

			if (syncSchedule == null)
			{
				syncSchedule = new LocationSyncSchedule
				{
					UserId = userId,
					LocationId = locationId,
					RecurringJobId = recurringJobId,
					LastSyncAt = DateTime.MinValue,
					NextSyncAt = DateTime.UtcNow.AddMinutes(refreshIntervalMinutes)
				};
				context.LocationSyncSchedules.Add(syncSchedule);
			}
			else
			{
				syncSchedule.RecurringJobId = recurringJobId;
				syncSchedule.NextSyncAt = DateTime.UtcNow.AddMinutes(refreshIntervalMinutes);
			}
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

	public async Task<RefreshResultDto> RefreshWeatherForUserTrackedLocationsAsync(Guid userId)
	{
		const int rateLimitMinutes = 15;

		// Get user preference to check rate limit
		var userPreference = await context.UserPreferences
			.FirstOrDefaultAsync(up => up.UserId == userId);

		if (userPreference == null)
		{
			throw new NotFoundException("User preference not found. Please set up your preferences first.");
		}

		// Check if rate limit is exceeded
		if (userPreference.LastManualRefreshAt.HasValue)
		{
			var timeSinceLastRefresh = DateTime.UtcNow - userPreference.LastManualRefreshAt.Value;
			if (timeSinceLastRefresh < TimeSpan.FromMinutes(rateLimitMinutes))
			{
				var retryAfter = TimeSpan.FromMinutes(rateLimitMinutes) - timeSinceLastRefresh;
				throw new RateLimitExceededException(retryAfter);
			}
		}

		// Trigger immediate sync
		await openWeatherService.SyncWeatherForUserTrackedLocationsAsync(userId);

		// Update last manual refresh time
		userPreference.LastManualRefreshAt = DateTime.UtcNow;
		await context.SaveChangesAsync();

		// Get the latest sync time from LocationSyncSchedules
		var lastSyncedAt = await context.LocationSyncSchedules
			.Where(lss => lss.UserId == userId)
			.MaxAsync(lss => (DateTime?)lss.LastSyncAt);

		return new RefreshResultDto
		{
			Success = true,
			Message = $"Weather data refreshed successfully for all your tracked locations.",
			LastSyncedAt = lastSyncedAt ?? DateTime.UtcNow,
			NextRefreshAllowedAt = DateTime.UtcNow.AddMinutes(rateLimitMinutes)
		};
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

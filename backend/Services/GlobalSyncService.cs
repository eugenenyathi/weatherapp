using Hangfire;
using weatherapp.Services.Interfaces;

namespace weatherapp.Services;

public class GlobalSyncService(IRecurringJobManager recurringJobManager, IOpenWeatherService openWeatherService) : IGlobalSyncService
{
	private const string GlobalSyncJobId = "global-locations-weather-sync";

	public Task InitializeGlobalSyncAsync()
	{
		// Register recurring job to sync all locations every hour
		// Cron expression "0 * * * *" means: at minute 0 of every hour
		recurringJobManager.AddOrUpdate(
			GlobalSyncJobId,
			() => openWeatherService.SyncLocationsDailyWeather(),
			"0 * * * *"
		);

		return Task.CompletedTask;
	}

	public Task RemoveGlobalSyncAsync()
	{
		recurringJobManager.RemoveIfExists(GlobalSyncJobId);
		return Task.CompletedTask;
	}
}

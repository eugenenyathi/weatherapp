using weatherapp.DataTransferObjects;

namespace weatherapp.Services.Interfaces;

public interface ISyncScheduleService
{
	Task InitializeSyncSchedulesForUserAsync(Guid userId, int refreshIntervalMinutes);
	Task UpdateSyncSchedulesForUserAsync(Guid userId, int newRefreshIntervalMinutes);
	Task RemoveSyncSchedulesForUserAsync(Guid userId);
	Task TriggerSyncForUserAsync(Guid userId);
	Task InitializeAllUserSyncSchedulesAsync();
}

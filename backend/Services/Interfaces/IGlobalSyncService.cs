namespace weatherapp.Services.Interfaces;

public interface IGlobalSyncService
{
	Task InitializeGlobalSyncAsync();
	Task RemoveGlobalSyncAsync();
}

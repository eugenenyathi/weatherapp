using weatherapp.DataTransferObjects;
using weatherapp.Requests;

namespace weatherapp.Services.Interfaces;

public interface ILocationService
{
	Task<List<LocationDto>> GetAllAsync();
	Task<LocationDto> CreateAsync(LocationRequest request);
	Task WaitForLocationWeatherDataAsync(Guid locationId, CancellationToken cancellationToken = default);
	void UpdateLocationJobStatus(Guid locationId, string jobId);
}

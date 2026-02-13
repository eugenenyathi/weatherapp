using weatherapp.DataTransferObjects;
using weatherapp.Requests;

namespace weatherapp.Services.Interfaces;

public interface ITrackLocationService
{
	Task<List<TrackLocationDto>> GetAllByUserIdAsync(string userId);
	Task<TrackLocationDto> CreateAsync(string userId, TrackLocationRequest request);
	Task<TrackLocationDto?> UpdateAsync(string userId, Guid locationId, TrackLocationRequest request);
	Task DeleteAsync(string userId, Guid locationId);
}
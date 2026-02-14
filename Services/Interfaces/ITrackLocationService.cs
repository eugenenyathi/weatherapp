using weatherapp.DataTransferObjects;
using weatherapp.Requests;

namespace weatherapp.Services.Interfaces;

public interface ITrackLocationService
{
	Task<List<TrackLocationDto>> GetAllByUserIdAsync(Guid userId);
	Task<TrackLocationDto> CreateAsync(Guid userId, TrackLocationRequest request);
	Task<TrackLocationDto?> UpdateAsync(Guid userId, Guid locationId, TrackLocationRequest request);
	Task DeleteAsync(Guid userId, Guid locationId);
}
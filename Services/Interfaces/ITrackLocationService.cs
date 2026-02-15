using weatherapp.DataTransferObjects;
using weatherapp.Requests;

namespace weatherapp.Services.Interfaces;

public interface ITrackLocationService
{
	Task<List<TrackLocationDto>> GetAllByUserIdAsync(Guid userId);
	Task<TrackLocationDto> CreateAsync(Guid userId, CreateTrackLocationRequest requests);
	Task<TrackLocationDto> UpdateAsync(Guid userId, Guid trackedLocationId, UpdateTrackLocationRequest requests);
	Task DeleteAsync(Guid userId, Guid trackedLocationId);
}
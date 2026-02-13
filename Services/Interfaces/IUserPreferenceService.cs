using weatherapp.DataTransferObjects;
using weatherapp.Requests;

namespace weatherapp.Services.Interfaces;

public interface IUserPreferenceService
{
	Task<UserPreferenceDto?> GetByUserIdAsync(string userId);
	Task<UserPreferenceDto> CreateAsync(string userId, UserPreferenceRequest request);
	Task<UserPreferenceDto> UpdateAsync(Guid preferenceId, UserPreferenceRequest request);
	Task DeleteAsync(Guid preferenceId);
}
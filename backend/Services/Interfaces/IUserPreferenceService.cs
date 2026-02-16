using weatherapp.DataTransferObjects;
using weatherapp.Requests;

namespace weatherapp.Services.Interfaces;

public interface IUserPreferenceService
{
	Task<UserPreferenceDto?> GetByUserIdAsync(Guid userId);
	Task<UserPreferenceDto> CreateAsync(Guid userId, UserPreferenceRequest request);
	Task<UserPreferenceDto> UpdateAsync(Guid userId, Guid preferenceId, UserPreferenceRequest request);
	Task DeleteAsync(Guid userId, Guid preferenceId);
}
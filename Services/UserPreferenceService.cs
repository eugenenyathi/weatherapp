using AutoMapper;
using Microsoft.EntityFrameworkCore;
using weatherapp.Data;
using weatherapp.DataTransferObjects;
using weatherapp.Entities;
using weatherapp.Enums;
using weatherapp.Requests;
using weatherapp.Services.Interfaces;

namespace weatherapp.Services;

public class UserPreferenceService(AppDbContext context, IMapper mapper) : IUserPreferenceService
{
	public async Task<UserPreferenceDto?> GetByUserIdAsync(Guid userId)
	{
		var userPreference = await context.UserPreferences
			.FirstOrDefaultAsync(up => up.UserId == userId);

		return userPreference == null ? null : mapper.Map<UserPreferenceDto>(userPreference);
	}

	public async Task<UserPreferenceDto> CreateAsync(Guid userId, UserPreferenceRequest request)
	{
		// Check if user preference already exists
		if (await context.UserPreferences.AnyAsync(up => up.UserId == userId))
			throw new InvalidOperationException(
				$"User preference already exists for user ID {userId}.");


		var userPreference = mapper.Map<UserPreference>(request);
		userPreference.UserId = userId;
		userPreference.PreferredUnit = request.PreferredUnit ?? Unit.Metric; // Default to Metric if not provided
		userPreference.RefreshInterval = request.RefreshInterval ?? 30; // Default to 30 minutes if not provided

		await context.UserPreferences.AddAsync(userPreference);
		await context.SaveChangesAsync();

		return mapper.Map<UserPreferenceDto>(userPreference);
	}

	public async Task<UserPreferenceDto> UpdateAsync(Guid preferenceId, UserPreferenceRequest request)
	{
		var userPreference = await context.UserPreferences
			.FirstOrDefaultAsync(up => up.Id == preferenceId) ?? throw new Exception("User preference not found.");

		mapper.Map(request, userPreference);
		await context.SaveChangesAsync();
		return mapper.Map<UserPreferenceDto>(userPreference);
	}

	public async Task DeleteAsync(Guid preferenceId)
	{
		var userPreference = await context.UserPreferences
			                     .FirstOrDefaultAsync(up => up.Id == preferenceId) ??
		                     throw new InvalidOperationException($"User preference not found for ID {preferenceId}");


		context.UserPreferences.Remove(userPreference);
		await context.SaveChangesAsync();
	}
}
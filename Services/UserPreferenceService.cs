using AutoMapper;
using Microsoft.EntityFrameworkCore;
using weatherapp.Data;
using weatherapp.DataTransferObjects;
using weatherapp.Entities;
using weatherapp.Enums;
using weatherapp.Exceptions;
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
		if(!await context.Users.AnyAsync(u => u.Id == userId))
			throw new Exception("User doesn't exist");
		// Check if user preference already exists
		if (await context.UserPreferences.AnyAsync(up => up.UserId == userId))
			throw new InvalidOperationException(
				$"User preference already exists for user ID {userId}.");

		var userPreference = new UserPreference
		{
			UserId = userId,
			PreferredUnit = request.PreferredUnit ?? Unit.Metric,// Default to Metric if not provided
			RefreshInterval = request.RefreshInterval ?? 30 // Default to 30 minutes if not provided
		};

		await context.UserPreferences.AddAsync(userPreference);
		await context.SaveChangesAsync();

		return mapper.Map<UserPreferenceDto>(userPreference);
	}

	public async Task<UserPreferenceDto> UpdateAsync(Guid userId, Guid preferenceId, UserPreferenceRequest request)
	{
		var userPreference = await context.UserPreferences
			.FirstOrDefaultAsync(up => up.UserId == userId && up.Id == preferenceId) 
			?? throw new NotFoundException($"User preference not found for user ID {userId} and preference ID {preferenceId}.");

		// Update only the fields that are provided in the request
		if (request.PreferredUnit.HasValue)
			userPreference.PreferredUnit = request.PreferredUnit.Value;

		if (request.RefreshInterval.HasValue)
			userPreference.RefreshInterval = request.RefreshInterval.Value;

		userPreference.UpdatedAt = DateTime.UtcNow;

		await context.SaveChangesAsync();

		return mapper.Map<UserPreferenceDto>(userPreference);
	}

	public async Task DeleteAsync(Guid userId, Guid preferenceId)
	{
		var userPreference = await context.UserPreferences
			.FirstOrDefaultAsync(up => up.UserId == userId && up.Id == preferenceId);

		if (userPreference == null) 
			throw new NotFoundException($"Failed to delete User Preference with ID {preferenceId} for user {userId}");

		context.UserPreferences.Remove(userPreference);
		await context.SaveChangesAsync();
	}
}
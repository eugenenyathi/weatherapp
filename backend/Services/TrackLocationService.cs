using AutoMapper;
using Microsoft.EntityFrameworkCore;
using weatherapp.Data;
using weatherapp.DataTransferObjects;
using weatherapp.Entities;
using weatherapp.Exceptions;
using weatherapp.Requests;
using weatherapp.Services.Interfaces;

namespace weatherapp.Services;

public class TrackLocationService(AppDbContext context, IMapper mapper) : ITrackLocationService
{
	public async Task<List<TrackLocationDto>> GetAllByUserIdAsync(Guid userId)
	{
		var trackLocations = await context.TrackLocations
			.Include(tl => tl.Location)
			.Where(tl => tl.UserId == userId)
			.ToListAsync();

		return mapper.Map<List<TrackLocationDto>>(trackLocations);
	}

	public async Task<TrackLocationDto> CreateAsync(Guid userId, CreateTrackLocationRequest requests)
	{
		// Check if the location exists
		var locationExists = await context.Locations.AnyAsync(l => l.Id == requests.LocationId);
		if (!locationExists)
			throw new NotFoundException($"Location with ID {requests.LocationId} does not exist.");

		// Check if the user is already tracking this location
		var existingTrackLocation = await context.TrackLocations
			.FirstOrDefaultAsync(tl => tl.UserId == userId && tl.LocationId == requests.LocationId);

		if (existingTrackLocation != null)
		{
			throw new DuplicateTrackLocationException(
				$"User is already tracking location with ID {requests.LocationId}.");
		}

		var trackLocation = mapper.Map<TrackLocation>(requests);
		trackLocation.UserId = userId;

		await context.TrackLocations.AddAsync(trackLocation);
		await context.SaveChangesAsync();

		// Check if weather data already exists for this location
		var hasWeatherData = await context.DailyWeathers
			.AnyAsync(dw => dw.LocationId == requests.LocationId);

		// Create or update LocationSyncSchedule
		var syncSchedule = await context.LocationSyncSchedules
			.FirstOrDefaultAsync(lss => lss.UserId == userId && lss.LocationId == requests.LocationId);

		if (syncSchedule == null)
		{
			syncSchedule = new LocationSyncSchedule
			{
				UserId = userId,
				LocationId = requests.LocationId,
				RecurringJobId = string.Empty,
				LastSyncAt = hasWeatherData ? DateTime.UtcNow : DateTime.MinValue,
				NextSyncAt = DateTime.UtcNow
			};
			context.LocationSyncSchedules.Add(syncSchedule);
		}
		else if (hasWeatherData && syncSchedule.LastSyncAt == DateTime.MinValue)
		{
			// Update LastSyncAt if weather data exists but wasn't recorded
			syncSchedule.LastSyncAt = DateTime.UtcNow;
		}

		await context.SaveChangesAsync();

		// Reload with location data
		var trackLocationWithLocation = await context.TrackLocations
			.Include(tl => tl.Location)
			.FirstOrDefaultAsync(tl => tl.Id == trackLocation.Id);

		return mapper.Map<TrackLocationDto>(trackLocationWithLocation);
	}

	public async Task<TrackLocationDto> UpdateAsync(Guid userId, Guid trackedLocationId, UpdateTrackLocationRequest requests)
	{
		var trackLocation = await context.TrackLocations
			                    .FirstOrDefaultAsync(tl => tl.UserId == userId && tl.Id == trackedLocationId) ??
		                    throw new NotFoundException($"Tracked location with ID {trackedLocationId} not found for user ID {userId}.");

		// Update only the fields that are provided in the request
		if (requests.IsFavorite.HasValue)
			trackLocation.isFavorite = requests.IsFavorite.Value;


		if (!string.IsNullOrEmpty(requests.DisplayName))
			trackLocation.DisplayName = requests.DisplayName;


		await context.SaveChangesAsync();

		// Reload with location data
		var updatedTrackLocation = await context.TrackLocations
			.Include(tl => tl.Location)
			.FirstOrDefaultAsync(tl => tl.Id == trackLocation.Id);

		return mapper.Map<TrackLocationDto>(updatedTrackLocation);
	}

	public async Task DeleteAsync(Guid userId, Guid trackedLocationId)
	{
		var trackLocation = await context.TrackLocations
			.FirstOrDefaultAsync(tl => tl.UserId == userId && tl.Id == trackedLocationId);

		if (trackLocation == null) throw new NotFoundException($"Failed to delete Tracked Location with ID {trackedLocationId}");

		context.TrackLocations.Remove(trackLocation);
		await context.SaveChangesAsync();
	}
}
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using weatherapp.Data;
using weatherapp.DataTransferObjects;
using weatherapp.Entities;
using weatherapp.Requests;
using weatherapp.Services.Interfaces;

namespace weatherapp.Services;

public class TrackLocationService(AppDbContext context, IMapper mapper) : ITrackLocationService
{
	public async Task<List<TrackLocationDto>> GetAllByUserIdAsync(string userId)
	{
		var trackLocations = await context.TrackLocations
			.Include(tl => tl.Location)
			.Where(tl => tl.UserId == userId)
			.ToListAsync();

		return mapper.Map<List<TrackLocationDto>>(trackLocations);
	}

	public async Task<TrackLocationDto> CreateAsync(string userId, TrackLocationRequest request)
	{
		// Check if the location exists
		var locationExists = await context.Locations.AnyAsync(l => l.Id == request.LocationId);
		if (!locationExists)
			throw new ArgumentException($"Location with ID {request.LocationId} does not exist.");

		// Check if the user is already tracking this location
		var existingTrackLocation = await context.TrackLocations
			.FirstOrDefaultAsync(tl => tl.UserId == userId && tl.LocationId == request.LocationId);

		if (existingTrackLocation != null)
		{
			throw new InvalidOperationException(
				$"User is already tracking location with ID {request.LocationId}.");
		}

		var trackLocation = mapper.Map<TrackLocation>(request);
		trackLocation.UserId = userId;

		await context.TrackLocations.AddAsync(trackLocation);
		await context.SaveChangesAsync();

		// Reload with location data
		var trackLocationWithLocation = await context.TrackLocations
			.Include(tl => tl.Location)
			.FirstOrDefaultAsync(tl => tl.Id == trackLocation.Id);

		return mapper.Map<TrackLocationDto>(trackLocationWithLocation);
	}

	public async Task<TrackLocationDto?> UpdateAsync(string userId, Guid locationId, TrackLocationRequest request)
	{
		var trackLocation = await context.TrackLocations
			                    .FirstOrDefaultAsync(tl => tl.UserId == userId && tl.LocationId == locationId) ??
		                    throw new ArgumentException($"Location with ID {request.LocationId} does not exist.");

		// Update only the fields that are provided in the request
		if (request.IsFavorite.HasValue)
			trackLocation.isFavorite = request.IsFavorite.Value;
		
		
		if (!string.IsNullOrEmpty(request.DisplayName))
			trackLocation.DisplayName = request.DisplayName;
		

		await context.SaveChangesAsync();

		// Reload with location data
		var updatedTrackLocation = await context.TrackLocations
			.Include(tl => tl.Location)
			.FirstOrDefaultAsync(tl => tl.Id == trackLocation.Id);

		return mapper.Map<TrackLocationDto>(updatedTrackLocation);
	}

	public async Task DeleteAsync(string userId, Guid locationId)
	{
		var trackLocation = await context.TrackLocations
			.FirstOrDefaultAsync(tl => tl.UserId == userId && tl.LocationId == locationId);

		if (trackLocation == null) throw new ArgumentException($"Failed to delete location with id {locationId}");

		context.TrackLocations.Remove(trackLocation);
		await context.SaveChangesAsync();
	}
}
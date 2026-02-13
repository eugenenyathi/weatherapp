using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using weatherapp.Data;
using weatherapp.DataTransferObjects;
using weatherapp.Entities;
using weatherapp.Requests;
using weatherapp.Services.Interfaces;

namespace weatherapp.Services;

public class LocationService(AppDbContext context, IMapper mapper) : ILocationService
{
	public async Task<List<LocationDto>> GetAllAsync()
	{
		return await context.Locations.ProjectTo<LocationDto>(mapper.ConfigurationProvider).ToListAsync();
	}

	public async Task<LocationDto> CreateAsync(LocationRequest request)
	{
		var location = mapper.Map<Location>(request);
		location.Id = Guid.NewGuid();
		location.CreatedAt = DateTime.UtcNow;
		location.UpdatedAt = DateTime.UtcNow;

		await context.Locations.AddAsync(location);
		await context.SaveChangesAsync();

		return mapper.Map<LocationDto>(location);
	}
}
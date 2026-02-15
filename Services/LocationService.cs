using AutoMapper;
using AutoMapper.QueryableExtensions;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using weatherapp.Data;
using weatherapp.DataTransferObjects;
using weatherapp.Entities;
using weatherapp.Requests;
using weatherapp.Services.Interfaces;

namespace weatherapp.Services;

public class LocationService(AppDbContext context, IMapper mapper, IBackgroundJobClient backgroundJobs, IOpenWeatherService openWeatherService) : ILocationService
{
	public async Task<List<LocationDto>> GetAllAsync()
	{
		return await context.Locations.ProjectTo<LocationDto>(mapper.ConfigurationProvider).ToListAsync();
	}

	public async Task<LocationDto> CreateAsync(LocationRequest request)
	{
		if (await context.Locations.AnyAsync(l => l.Name == request.Name))
		{
			return (await context.Locations.Where(l => l.Name == request.Name)
				.ProjectTo<LocationDto>(mapper.ConfigurationProvider).FirstOrDefaultAsync())!;
		}

		var location = mapper.Map<Location>(request);

		await context.Locations.AddAsync(location);
		await context.SaveChangesAsync();

		// Enqueue a background job to fetch daily weather for the newly created location
		backgroundJobs.Enqueue(() => openWeatherService.GetLocationDailyWeather(location));

		return mapper.Map<LocationDto>(location);
	}
}
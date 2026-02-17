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

public class LocationService(
	AppDbContext context,
	IMapper mapper,
	IBackgroundJobClient backgroundJobs,
	IOpenWeatherService openWeatherService) : ILocationService
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

		// Enqueue background jobs to fetch daily and hourly weather for the newly created location
		var dailyJobId = backgroundJobs.Enqueue(() => openWeatherService.GetLocationDailyWeather(location));
		var hourlyJobId = backgroundJobs.Enqueue(() => openWeatherService.GetLocationHourlyWeather(location));

		// Store the job tracking in LocationJob entity for daily weather
		var locationJob = new LocationJob
		{
			LocationId = location.Id,
			JobId = dailyJobId,
			JobCreatedAt = DateTime.UtcNow,
			Status = "Pending"
		};

		await context.LocationJobs.AddAsync(locationJob);
		await context.SaveChangesAsync();

		// Enqueue continuation jobs to update the status after the weather fetch completes
		backgroundJobs.ContinueJobWith(dailyJobId, () => UpdateLocationJobStatus(location.Id, dailyJobId));

		return mapper.Map<LocationDto>(location);
	}

	public void UpdateLocationJobStatus(Guid locationId, string jobId)
	{
		var locationJob = context.LocationJobs
			.FirstOrDefault(lj => lj.LocationId == locationId && lj.JobId == jobId);
		
		if (locationJob != null)
		{
			locationJob.Status = "Completed";
			context.SaveChanges();
		}
	}

	public async Task WaitForLocationWeatherDataAsync(Guid locationId, CancellationToken cancellationToken = default)
	{
		// Get the most recent pending job for this location
		var locationJob = await context.LocationJobs
			.Where(lj => lj.LocationId == locationId && lj.Status == "Pending" || lj.Status == "Processing")
			.OrderByDescending(lj => lj.JobCreatedAt)
			.FirstOrDefaultAsync(cancellationToken);

		if (locationJob == null)
		{
			return; // No pending job to wait for
		}

		// Check if job is still enqueued or processing using Hangfire's monitoring API
		var monitoringApi = JobStorage.Current.GetMonitoringApi();

		// Wait for job to complete (max 30 seconds)
		var maxWaitTime = TimeSpan.FromSeconds(30);
		var startTime = DateTime.UtcNow;

		while (DateTime.UtcNow - startTime < maxWaitTime)
		{
			cancellationToken.ThrowIfCancellationRequested();

			// Get job details (synchronous call)
			var jobDetails = monitoringApi.JobDetails(locationJob.JobId);

			if (jobDetails == null)
			{
				// Job doesn't exist in Hangfire storage, might have been cleaned up
				// Check if weather data exists as a fallback
				var hasWeatherData = await context.DailyWeathers
					.AnyAsync(dw => dw.LocationId == locationId, cancellationToken);

				if (!hasWeatherData)
				{
					// Trigger a new job if no weather data exists
					var location = await context.Locations.FindAsync([locationId], cancellationToken);
					if (location != null)
					{
						var jobId = backgroundJobs.Enqueue(() => openWeatherService.GetLocationDailyWeather(location));
						var newLocationJob = new LocationJob
						{
							LocationId = locationId,
							JobId = jobId,
							JobCreatedAt = DateTime.UtcNow,
							Status = "Pending"
						};
						await context.LocationJobs.AddAsync(newLocationJob, cancellationToken);
						await context.SaveChangesAsync(cancellationToken);
					}
				}

				// Update job status
				locationJob.Status = "Completed";
				await context.SaveChangesAsync(cancellationToken);
				break;
			}

			var jobHistory = jobDetails.History.OrderByDescending(h => h.CreatedAt).ToList();
			if (jobHistory.Count == 0)
			{
				await Task.Delay(500, cancellationToken);
				continue;
			}

			var currentState = jobHistory.First().StateName;

			// Update the job status in database
			locationJob.Status = currentState;

			// If job is in a final state (Succeeded, Deleted, Failed), stop waiting
			if (currentState is "Succeeded" or "Deleted" or "Failed")
			{
				await context.SaveChangesAsync(cancellationToken);
				break;
			}

			await context.SaveChangesAsync(cancellationToken);

			// Job is still enqueued or processing, wait a bit
			await Task.Delay(500, cancellationToken);
		}
	}
}
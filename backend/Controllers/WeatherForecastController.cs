using Microsoft.AspNetCore.Mvc;
using weatherapp.DataTransferObjects;
using weatherapp.Services.Interfaces;

namespace weatherapp.Controllers;

[ApiController]
[Route("api/weather-forecasts")]
public class
	WeatherForecastController(IWeatherForecastService weatherForecastService, ISyncScheduleService syncScheduleService)
	: ControllerBase
{
	[HttpGet("current-day-summaries/{userId}")]
	public async Task<ActionResult<List<LocationWeatherSummaryDto>>>
		GetCurrentDaySummariesForAllTrackedLocations(Guid userId)
	{
		var summaries = await weatherForecastService.GetCurrentDaySummariesForAllTrackedLocationsAsync(userId);
		return Ok(summaries);
	}

	[HttpGet("five-day-forecast/{locationId}/{userId}")]
	public async Task<ActionResult<LocationFiveDayForecastDto>> GetFiveDayForecastForLocation(Guid locationId,
		Guid userId)
	{
		var forecast = await weatherForecastService.GetFiveDayForecastForLocationAsync(locationId, userId);
		return Ok(forecast);
	}

	[HttpGet("hourly-forecast/{locationId}/{userId}")]
	public async Task<ActionResult<LocationHourlyForecastDto>> GetHourlyForecastForLocation(Guid locationId,
		Guid userId)
	{
		var forecast = await weatherForecastService.GetHourlyForecastForLocationAsync(locationId, userId);
		return Ok(forecast);
	}

	[HttpPost("refresh/{userId}")]
	public async Task<ActionResult<RefreshResultDto>> RefreshWeatherData(Guid userId)
	{
		var result = await syncScheduleService.RefreshWeatherForUserTrackedLocationsAsync(userId);

		// Success - include last synced time in headers
		if (result.LastSyncedAt.HasValue)
		{
			Response.Headers.Append("X-Last-Synced", result.LastSyncedAt.Value.ToString("o"));
		}

		return Ok(result);
	}
}
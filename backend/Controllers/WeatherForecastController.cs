using Microsoft.AspNetCore.Mvc;
using weatherapp.Services.Interfaces;
using weatherapp.DataTransferObjects;

namespace weatherapp.Controllers;

[ApiController]
[Route("api/weather-forecasts")]
public class WeatherForecastController(IWeatherForecastService weatherForecastService) : ControllerBase
{

    [HttpGet("current-day-summaries/{userId}")]
    public async Task<ActionResult<List<LocationWeatherSummaryDto>>> GetCurrentDaySummariesForAllTrackedLocations(Guid userId)
    {
        var summaries = await weatherForecastService.GetCurrentDaySummariesForAllTrackedLocationsAsync(userId);
        return Ok(summaries);
    }

    [HttpGet("five-day-forecast/{locationId}/{userId}")]
    public async Task<ActionResult<LocationFiveDayForecastDto>> GetFiveDayForecastForLocation(Guid locationId, Guid userId)
    {
        var forecast = await weatherForecastService.GetFiveDayForecastForLocationAsync(locationId, userId);
        return Ok(forecast);
    }

    [HttpGet("hourly-forecast/{locationId}/{userId}")]
    public async Task<ActionResult<LocationHourlyForecastDto>> GetHourlyForecastForLocation(Guid locationId, Guid userId)
    {
        var forecast = await weatherForecastService.GetHourlyForecastForLocationAsync(locationId, userId);
        return Ok(forecast);
    }
}
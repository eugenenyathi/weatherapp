using Microsoft.AspNetCore.Mvc;
using weatherapp.Services.Interfaces;
using weatherapp.DataTransferObjects;

namespace weatherapp.Controllers;

[ApiController]
[Route("api/weather-forecasts")]
public class WeatherForecastController(IWeatherForecastService weatherForecastService) : ControllerBase
{
    [HttpGet("tracked/{userId}")]
    public async Task<ActionResult<List<DailyWeatherDto>>> GetTrackedForecasts(Guid userId)
    {
        var forecasts = await weatherForecastService.GetTrackedForecastsAsync(userId);
        return Ok(forecasts);
    }
}
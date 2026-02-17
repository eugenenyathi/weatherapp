using Microsoft.EntityFrameworkCore;
using weatherapp.Data;
using weatherapp.DataTransferObjects;
using weatherapp.Entities;
using weatherapp.Enums;
using weatherapp.Services.Interfaces;

namespace weatherapp.Services;

public class OpenWeatherService(
    AppDbContext context,
    IOpenWeatherMapHttpClient httpClient,
    IConfiguration configuration) : IOpenWeatherService
{
    public async Task GetLocationHourlyWeather(Location location)
    {
        var apiKey = GetApiKey();
        // Note: 'daily' is excluded here, and we include 'hourly'
        var endpoint =
            $"?lat={location.Latitude}&lon={location.Longitude}&units=metric&exclude=current,minutely,daily,alerts&appid={apiKey}";

        var response = await httpClient.SendAsync(HttpMethod.Get, endpoint);
        response.EnsureSuccessStatusCode();

        var weatherData = await response.Content.ReadFromJsonAsync<OpenWeatherHourlyResponse>();
        if (weatherData?.Hourly == null) return;

        // Mapping to Hourly Entities (Taking the next 24 hours)
        var hourlyEntities = weatherData.Hourly.Take(24).Select(h => new HourWeather
        {
            LocationId = location.Id,
            DateTime = DateTimeOffset.FromUnixTimeSeconds(h.Dt).UtcDateTime,
            TempMetric = h.Temp,
            TempImperial = (h.Temp * 9 / 5) + 32,
            Humidity = h.Humidity,
        }).ToList();

        // Cleanup existing hourly data for this location to keep the DB fresh
        var existing = context.HourlyWeathers.Where(w => w.LocationId == location.Id);
        context.HourlyWeathers.RemoveRange(existing);

        await context.HourlyWeathers.AddRangeAsync(hourlyEntities);
        await context.SaveChangesAsync();
    }

    public async Task GetLocationDailyWeather(Location location)
    {
        var apiKey = GetApiKey();

        // Fetch in Metric by default
        var endpoint =
            $"?lat={location.Latitude}&lon={location.Longitude}&units=metric&exclude=current,minutely,hourly,alerts&appid={apiKey}";

        var response = await httpClient.SendAsync(HttpMethod.Get, endpoint);

        response.EnsureSuccessStatusCode();

        var weatherData = await response.Content.ReadFromJsonAsync<OpenWeatherDailyResponse>();

        if (weatherData?.Daily == null)
            return;

        var forecastEntities = weatherData.Daily
            .Take(5)
            .Select(d =>
            {
                var forecastTime = DateTimeOffset.FromUnixTimeSeconds(d.Dt);

                return new DayWeather
                {
                    LocationId = location.Id,
                    Date = DateOnly.FromDateTime(forecastTime.DateTime),
                    TimeOfForecast = forecastTime.DateTime,
                    MinTempMetric = d.Temp.Min,
                    MaxTempMetric = d.Temp.Max,
                    // Imperial (Calculated: C * 9/5 + 32)
                    MinTempImperial = (d.Temp.Min * 9 / 5) + 32,
                    MaxTempImperial = (d.Temp.Max * 9 / 5) + 32,
                    Humidity = d.Humidity,
                    Rain = d.Rain,
                    Summary = d.Summary
                };
            })
            .ToList();

        // Prevent duplicate dates for the same location
        var forecastDates = forecastEntities.Select(f => f.Date).ToList();

        var existingForecasts = context.DailyWeathers
            .Where(w => w.LocationId == location.Id &&
                        forecastDates.Contains(w.Date));

        context.DailyWeathers.RemoveRange(existingForecasts);

        await context.DailyWeathers.AddRangeAsync(forecastEntities);

        await context.SaveChangesAsync();
    }

    public async Task SyncLocationsDailyWeather()
    {
        // 1. Grab all locations from the DB
        var locations = await context.Locations.ToListAsync();

        if (!locations.Any()) return;

        // 2. Iterate and update each
        foreach (var location in locations)
        {
            try
            {
                // Reusing the method you've already refined
                await GetLocationDailyWeather(location);
            }
            catch (Exception ex)
            {
                // Log the error for this specific location but continue with others
                Console.WriteLine($"Failed to sync weather for {location.Name}: {ex.Message}");
            }
        }
    }

    public async Task SyncWeatherForUserTrackedLocationsAsync(Guid userId)
    {
        // 1. Get all locations that this user is tracking
        var trackedLocations = await context.TrackLocations
            .Include(tl => tl.Location)
            .Where(tl => tl.UserId == userId)
            .Select(tl => tl.Location)
            .ToListAsync();

        if (!trackedLocations.Any()) return;

        // 2. Iterate and update each location
        foreach (var location in trackedLocations)
        {
            try
            {
                await GetLocationDailyWeather(location);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to sync weather for {location.Name} (User {userId}): {ex.Message}");
            }
        }
    }

    private string GetApiKey()
    {
        return configuration["OpenWeatherAPIKey"] ?? throw new ArgumentNullException("OpenWeatherAPIKey");
    }
}
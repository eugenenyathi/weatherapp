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
    IConfiguration configuration,
    ILogger<OpenWeatherService> logger) : IOpenWeatherService
{
    public async Task GetLocationHourlyWeather(Location location)
    {
        var apiKey = GetApiKey();
        var endpoint =
            $"?lat={location.Latitude}&lon={location.Longitude}&units=metric&exclude=current,minutely,daily,alerts&appid={apiKey}";

        var response = await httpClient.SendAsync(HttpMethod.Get, endpoint);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            logger.LogWarning(
                "OpenWeatherMap API returned {StatusCode} for location {LocationName} ({LocationId}): {ErrorContent}",
                response.StatusCode, location.Name, location.Id, errorContent);
            return;
        }

        var weatherData = await response.Content.ReadFromJsonAsync<OpenWeatherHourlyResponse>();

        if (weatherData?.Hourly == null)
        {
            logger.LogWarning("No hourly weather data received for location {LocationName} ({LocationId})",
                location.Name, location.Id);
            return;
        }

        // Mapping to Hourly Entities (Taking the next 24 hours)
        var hourlyEntities = weatherData.Hourly.Take(24).Select(h => new HourWeather
        {
            LocationId = location.Id,
            DateTime = DateTimeOffset.FromUnixTimeSeconds(h.Dt).UtcDateTime,
            TempMetric = h.Temp,
            TempImperial = (h.Temp * 9 / 5) + 32,
            Humidity = h.Humidity,
        }).ToList();

        // Prevent duplicate date-times for the same location
        var hourlyDateTimes = hourlyEntities.Select(h => h.DateTime).ToList();

        var existingHourly = context.HourlyWeathers
            .Where(w => w.LocationId == location.Id &&
                        hourlyDateTimes.Contains(w.DateTime));

        context.HourlyWeathers.RemoveRange(existingHourly);

        await context.HourlyWeathers.AddRangeAsync(hourlyEntities);

        await context.SaveChangesAsync();

        // Update LastSyncAt for all users tracking this location
        var syncSchedules = await context.LocationSyncSchedules
            .Where(lss => lss.LocationId == location.Id)
            .ToListAsync();

        foreach (var syncSchedule in syncSchedules)
        {
            syncSchedule.LastSyncAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();

        logger.LogDebug("Successfully fetched hourly weather for location {LocationName} ({LocationId})",
            location.Name, location.Id);
    }

    public async Task GetLocationDailyWeather(Location location)
    {
        var apiKey = GetApiKey();
        var endpoint =
            $"?lat={location.Latitude}&lon={location.Longitude}&units=metric&exclude=current,minutely,hourly,alerts&appid={apiKey}";

        var response = await httpClient.SendAsync(HttpMethod.Get, endpoint);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            logger.LogWarning(
                "OpenWeatherMap API returned {StatusCode} for location {LocationName} ({LocationId}): {ErrorContent}",
                response.StatusCode, location.Name, location.Id, errorContent);
            return;
        }

        var weatherData = await response.Content.ReadFromJsonAsync<OpenWeatherDailyResponse>();

        if (weatherData?.Daily == null)
        {
            logger.LogWarning("No daily weather data received for location {LocationName} ({LocationId})",
                location.Name, location.Id);
            return;
        }

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

        // Update LastSyncAt for all users tracking this location
        var syncSchedules = await context.LocationSyncSchedules
            .Where(lss => lss.LocationId == location.Id)
            .ToListAsync();

        foreach (var syncSchedule in syncSchedules)
        {
            syncSchedule.LastSyncAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();

        logger.LogDebug("Successfully fetched daily weather for location {LocationName} ({LocationId})",
            location.Name, location.Id);
    }

    public async Task SyncLocationsDailyWeather()
    {
        var locations = await context.Locations.ToListAsync();

        if (!locations.Any())
        {
            logger.LogInformation("No locations found to sync weather data");
            return;
        }

        logger.LogInformation("Starting weather sync for {Count} locations", locations.Count);

        foreach (var location in locations)
        {
            try
            {
                await GetLocationDailyWeather(location);
                await GetLocationHourlyWeather(location);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync weather for location {LocationName} ({LocationId})",
                    location.Name, location.Id);
            }
        }

        logger.LogInformation("Completed weather sync for {Count} locations", locations.Count);
    }

    public async Task SyncWeatherForUserTrackedLocationsAsync(Guid userId)
    {
        var trackedLocations = await context.TrackLocations
            .Include(tl => tl.Location)
            .Where(tl => tl.UserId == userId)
            .Select(tl => tl.Location)
            .ToListAsync();

        if (!trackedLocations.Any())
        {
            logger.LogInformation("No tracked locations found for user {UserId}", userId);
            return;
        }

        logger.LogInformation("Starting weather sync for user {UserId} with {Count} locations", userId, trackedLocations.Count);

        foreach (var location in trackedLocations)
        {
            try
            {
                await GetLocationDailyWeather(location);
                await GetLocationHourlyWeather(location);

                // Update the LastSyncAt for this user-location pair
                var syncSchedule = await context.LocationSyncSchedules
                    .FirstOrDefaultAsync(lss => lss.UserId == userId && lss.LocationId == location.Id);

                if (syncSchedule != null)
                {
                    syncSchedule.LastSyncAt = DateTime.UtcNow;
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync weather for location {LocationName} ({LocationId}) for user {UserId}",
                    location.Name, location.Id, userId);
            }
        }

        logger.LogInformation("Completed weather sync for user {UserId}", userId);
    }

    private string GetApiKey()
    {
        return configuration["OpenWeatherAPIKey"] ?? throw new ArgumentNullException("OpenWeatherAPIKey");
    }
}

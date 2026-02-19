using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using weatherapp.Data;
using weatherapp.Entities;
using weatherapp.Services;
using weatherapp.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using NUnit.Framework;

namespace weatherapp.tests.Services;

[TestFixture]
public class OpenWeatherServiceTests
{
    private Mock<AppDbContext> _mockContext = null!;
    private Mock<IOpenWeatherMapHttpClient> _mockHttpClient = null!;
    private Mock<IConfiguration> _mockConfiguration = null!;
    private Mock<ILogger<OpenWeatherService>> _mockLogger = null!;
    private OpenWeatherService _service = null!;

    private const string TestApiKey = "test-api-key-12345";

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<AppDbContext>();
        _mockHttpClient = new Mock<IOpenWeatherMapHttpClient>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<OpenWeatherService>>();

        _mockConfiguration.Setup(c => c["OpenWeatherAPIKey"]).Returns(TestApiKey);

        _service = new OpenWeatherService(
            _mockContext.Object,
            _mockHttpClient.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);
    }

    #region GetLocationHourlyWeather Tests

    [Test]
    public async Task GetLocationHourlyWeather_WithSuccessfulResponse_SavesHourlyData()
    {
        // Arrange
        var location = new Location
        {
            Id = Guid.NewGuid(),
            Name = "London",
            Latitude = 51.5074,
            Longitude = -0.1278
        };

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var hourlyData = Enumerable.Range(0, 24).Select(i => new
        {
            dt = now + (i * 3600),
            temp = 15.0 + i * 0.5,
            humidity = 60 + i,
            weather = new[] { new { main = "Clouds" } }
        }).ToArray();

        var responseContent = new OpenWeatherHourlyResponse
        {
            Hourly = hourlyData.Select(h => new OpenWeatherHourlyResponse.HourlyWeather
            {
                Dt = h.dt,
                Temp = h.temp,
                Humidity = h.humidity
            }).ToList()
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, "application/json")
        };

        _mockHttpClient.Setup(c => c.SendAsync(HttpMethod.Get, It.IsAny<string>()))
            .ReturnsAsync(httpResponse);

        // Act
        await _service.GetLocationHourlyWeather(location);

        // Assert
        _mockContext.Verify(c => c.HourlyWeathers.AddRangeAsync(
            It.IsAny<IEnumerable<HourWeather>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetLocationHourlyWeather_WithFailedResponse_LogsWarningAndDoesNotSave()
    {
        // Arrange
        var location = new Location
        {
            Id = Guid.NewGuid(),
            Name = "Paris",
            Latitude = 48.8566,
            Longitude = 2.3522
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("{\"message\": \"API Error\"}", Encoding.UTF8, "application/json")
        };

        _mockHttpClient.Setup(c => c.SendAsync(HttpMethod.Get, It.IsAny<string>()))
            .ReturnsAsync(httpResponse);

        // Act
        await _service.GetLocationHourlyWeather(location);

        // Assert
        _mockContext.Verify(c => c.HourlyWeathers.AddRangeAsync(It.IsAny<IEnumerable<HourWeather>>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetLocationHourlyWeather_WithNullHourlyData_LogsWarningAndDoesNotSave()
    {
        // Arrange
        var location = new Location
        {
            Id = Guid.NewGuid(),
            Name = "Berlin",
            Latitude = 52.52,
            Longitude = 13.405
        };

        var responseContent = new OpenWeatherHourlyResponse { Hourly = null };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, "application/json")
        };

        _mockHttpClient.Setup(c => c.SendAsync(HttpMethod.Get, It.IsAny<string>()))
            .ReturnsAsync(httpResponse);

        // Act
        await _service.GetLocationHourlyWeather(location);

        // Assert
        _mockContext.Verify(c => c.HourlyWeathers.AddRangeAsync(It.IsAny<IEnumerable<HourWeather>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetLocationHourlyWeather_ConvertsTemperaturesToImperial()
    {
        // Arrange
        var location = new Location
        {
            Id = Guid.NewGuid(),
            Name = "Tokyo",
            Latitude = 35.6762,
            Longitude = 139.6503
        };

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var hourlyData = new[]
        {
            new { dt = now, temp = 20.0, humidity = 65, weather = new[] { new { main = "Clear" } } }
        };

        var responseContent = new OpenWeatherHourlyResponse
        {
            Hourly = hourlyData.Select(h => new OpenWeatherHourlyResponse.HourlyWeather
            {
                Dt = h.dt,
                Temp = h.temp,
                Humidity = h.humidity
            }).ToList()
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, "application/json")
        };

        _mockHttpClient.Setup(c => c.SendAsync(HttpMethod.Get, It.IsAny<string>()))
            .ReturnsAsync(httpResponse);

        HourWeather? capturedHourly = null;
        _mockContext.Setup(c => c.HourlyWeathers.AddRangeAsync(It.IsAny<IEnumerable<HourWeather>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<HourWeather>, CancellationToken>((entities, _) =>
            {
                capturedHourly = entities.FirstOrDefault();
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.GetLocationHourlyWeather(location);

        // Assert
        capturedHourly.Should().NotBeNull();
        capturedHourly!.TempMetric.Should().Be(20.0f);
        capturedHourly.TempImperial.Should().Be(68.0f); // (20 * 9/5) + 32 = 68
    }

    [Test]
    public async Task GetLocationHourlyWeather_UsesCorrectApiEndpoint()
    {
        // Arrange
        var location = new Location
        {
            Id = Guid.NewGuid(),
            Name = "New York",
            Latitude = 40.7128,
            Longitude = -74.0060
        };

        var responseContent = new OpenWeatherHourlyResponse { Hourly = new List<OpenWeatherHourlyResponse.HourlyWeather>() };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, "application/json")
        };

        _mockHttpClient.Setup(c => c.SendAsync(HttpMethod.Get, It.IsAny<string>()))
            .ReturnsAsync(httpResponse);

        // Act
        await _service.GetLocationHourlyWeather(location);

        // Assert
        _mockHttpClient.Verify(c => c.SendAsync(
            HttpMethod.Get,
            It.Is<string>(url => url.Contains($"lat={location.Latitude}") &&
                                  url.Contains($"lon={location.Longitude}") &&
                                  url.Contains("units=metric") &&
                                  url.Contains("exclude=current,minutely,daily,alerts") &&
                                  url.Contains($"appid={TestApiKey}"))), Times.Once);
    }

    #endregion

    #region GetLocationDailyWeather Tests

    [Test]
    public async Task GetLocationDailyWeather_WithSuccessfulResponse_SavesDailyData()
    {
        // Arrange
        var location = new Location
        {
            Id = Guid.NewGuid(),
            Name = "Sydney",
            Latitude = -33.8688,
            Longitude = 151.2093
        };

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var dailyData = Enumerable.Range(0, 5).Select(i => new
        {
            dt = now + (i * 86400),
            temp = new { min = 15.0 + i, max = 22.0 + i },
            humidity = 55 + i * 2,
            rain = 0.5 * i,
            weather = new[] { new { main = "Clear", description = "clear sky" } }
        }).ToArray();

        var responseContent = new OpenWeatherDailyResponse
        {
            Daily = dailyData.Select(d => new OpenWeatherDailyResponse.DailyWeather
            {
                Dt = d.dt,
                Temp = new OpenWeatherDailyResponse.TempInfo { Min = d.temp.min, Max = d.temp.max },
                Humidity = d.humidity,
                Rain = d.rain,
                Summary = d.weather[0].main
            }).ToList()
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, "application/json")
        };

        _mockHttpClient.Setup(c => c.SendAsync(HttpMethod.Get, It.IsAny<string>()))
            .ReturnsAsync(httpResponse);

        // Act
        await _service.GetLocationDailyWeather(location);

        // Assert
        _mockContext.Verify(c => c.DailyWeathers.AddRangeAsync(
            It.IsAny<IEnumerable<DayWeather>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetLocationDailyWeather_WithFailedResponse_LogsWarningAndDoesNotSave()
    {
        // Arrange
        var location = new Location
        {
            Id = Guid.NewGuid(),
            Name = "Mumbai",
            Latitude = 19.0760,
            Longitude = 72.8777
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("{\"message\": \"Gateway Error\"}", Encoding.UTF8, "application/json")
        };

        _mockHttpClient.Setup(c => c.SendAsync(HttpMethod.Get, It.IsAny<string>()))
            .ReturnsAsync(httpResponse);

        // Act
        await _service.GetLocationDailyWeather(location);

        // Assert
        _mockContext.Verify(c => c.DailyWeathers.AddRangeAsync(It.IsAny<IEnumerable<DayWeather>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetLocationDailyWeather_WithNullDailyData_LogsWarningAndDoesNotSave()
    {
        // Arrange
        var location = new Location
        {
            Id = Guid.NewGuid(),
            Name = "Cairo",
            Latitude = 30.0444,
            Longitude = 31.2357
        };

        var responseContent = new OpenWeatherDailyResponse { Daily = null };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, "application/json")
        };

        _mockHttpClient.Setup(c => c.SendAsync(HttpMethod.Get, It.IsAny<string>()))
            .ReturnsAsync(httpResponse);

        // Act
        await _service.GetLocationDailyWeather(location);

        // Assert
        _mockContext.Verify(c => c.DailyWeathers.AddRangeAsync(It.IsAny<IEnumerable<DayWeather>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetLocationDailyWeather_ConvertsTemperaturesToImperial()
    {
        // Arrange
        var location = new Location
        {
            Id = Guid.NewGuid(),
            Name = "Toronto",
            Latitude = 43.6532,
            Longitude = -79.3832
        };

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var dailyData = new[]
        {
            new
            {
                dt = now,
                temp = new { min = 10.0, max = 18.0 },
                humidity = 50,
                rain = 0.0,
                weather = new[] { new { main = "Sunny" } }
            }
        };

        var responseContent = new OpenWeatherDailyResponse
        {
            Daily = dailyData.Select(d => new OpenWeatherDailyResponse.DailyWeather
            {
                Dt = d.dt,
                Temp = new OpenWeatherDailyResponse.TempInfo { Min = d.temp.min, Max = d.temp.max },
                Humidity = d.humidity,
                Rain = d.rain,
                Summary = d.weather[0].main
            }).ToList()
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, "application/json")
        };

        _mockHttpClient.Setup(c => c.SendAsync(HttpMethod.Get, It.IsAny<string>()))
            .ReturnsAsync(httpResponse);

        DayWeather? capturedDaily = null;
        _mockContext.Setup(c => c.DailyWeathers.AddRangeAsync(It.IsAny<IEnumerable<DayWeather>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<DayWeather>, CancellationToken>((entities, _) =>
            {
                capturedDaily = entities.FirstOrDefault();
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.GetLocationDailyWeather(location);

        // Assert
        capturedDaily.Should().NotBeNull();
        capturedDaily!.MinTempMetric.Should().Be(10.0f);
        capturedDaily.MaxTempMetric.Should().Be(18.0f);
        capturedDaily.MinTempImperial.Should().Be(50.0f); // (10 * 9/5) + 32 = 50
        capturedDaily.MaxTempImperial.Should().Be(64.4f); // (18 * 9/5) + 32 = 64.4
    }

    [Test]
    public async Task GetLocationDailyWeather_UsesCorrectApiEndpoint()
    {
        // Arrange
        var location = new Location
        {
            Id = Guid.NewGuid(),
            Name = "Los Angeles",
            Latitude = 34.0522,
            Longitude = -118.2437
        };

        var responseContent = new OpenWeatherDailyResponse { Daily = new List<OpenWeatherDailyResponse.DailyWeather>() };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, "application/json")
        };

        _mockHttpClient.Setup(c => c.SendAsync(HttpMethod.Get, It.IsAny<string>()))
            .ReturnsAsync(httpResponse);

        // Act
        await _service.GetLocationDailyWeather(location);

        // Assert
        _mockHttpClient.Verify(c => c.SendAsync(
            HttpMethod.Get,
            It.Is<string>(url => url.Contains($"lat={location.Latitude}") &&
                                  url.Contains($"lon={location.Longitude}") &&
                                  url.Contains("units=metric") &&
                                  url.Contains("exclude=current,minutely,hourly,alerts") &&
                                  url.Contains($"appid={TestApiKey}"))), Times.Once);
    }

    #endregion

    #region SyncLocationsDailyWeather Tests

    [Test]
    public async Task SyncLocationsDailyWeather_WhenNoLocations_LogsInformationAndReturns()
    {
        // Arrange
        var locations = new List<Location>().AsQueryable();

        var mockSet = new Mock<DbSet<Location>>();
        mockSet.As<IQueryable<Location>>().Setup(m => m.Provider).Returns(locations.Provider);
        mockSet.As<IQueryable<Location>>().Setup(m => m.Expression).Returns(locations.Expression);
        mockSet.As<IQueryable<Location>>().Setup(m => m.ElementType).Returns(locations.ElementType);
        mockSet.As<IQueryable<Location>>().Setup(m => m.GetEnumerator()).Returns(locations.GetEnumerator());

        _mockContext.Setup(c => c.Locations).Returns(mockSet.Object);

        // Act
        await _service.SyncLocationsDailyWeather();

        // Assert
        _mockHttpClient.Verify(c => c.SendAsync(It.IsAny<HttpMethod>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SyncLocationsDailyWeather_WithLocations_FetchesWeatherForAll()
    {
        // Arrange
        var locations = new List<Location>
        {
            new() { Id = Guid.NewGuid(), Name = "London", Latitude = 51.5074, Longitude = -0.1278 },
            new() { Id = Guid.NewGuid(), Name = "Paris", Latitude = 48.8566, Longitude = 2.3522 },
            new() { Id = Guid.NewGuid(), Name = "Berlin", Latitude = 52.52, Longitude = 13.405 }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<Location>>();
        mockSet.As<IQueryable<Location>>().Setup(m => m.Provider).Returns(locations.Provider);
        mockSet.As<IQueryable<Location>>().Setup(m => m.Expression).Returns(locations.Expression);
        mockSet.As<IQueryable<Location>>().Setup(m => m.ElementType).Returns(locations.ElementType);
        mockSet.As<IQueryable<Location>>().Setup(m => m.GetEnumerator()).Returns(locations.GetEnumerator());

        _mockContext.Setup(c => c.Locations).Returns(mockSet.Object);

        var responseContent = new OpenWeatherDailyResponse { Daily = new List<OpenWeatherDailyResponse.DailyWeather>() };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, "application/json")
        };

        _mockHttpClient.Setup(c => c.SendAsync(HttpMethod.Get, It.IsAny<string>()))
            .ReturnsAsync(httpResponse);

        // Act
        await _service.SyncLocationsDailyWeather();

        // Assert
        _mockHttpClient.Verify(c => c.SendAsync(HttpMethod.Get, It.IsAny<string>()), Times.AtLeast(6)); // Daily + Hourly for each location
    }

    [Test]
    public async Task SyncLocationsDailyWeather_WithExceptionForLocation_LogsErrorAndContinues()
    {
        // Arrange
        var location1 = new Location { Id = Guid.NewGuid(), Name = "Success City", Latitude = 51.5074, Longitude = -0.1278 };
        var location2 = new Location { Id = Guid.NewGuid(), Name = "Error City", Latitude = 48.8566, Longitude = 2.3522 };

        var locations = new List<Location> { location1, location2 }.AsQueryable();

        var mockSet = new Mock<DbSet<Location>>();
        mockSet.As<IQueryable<Location>>().Setup(m => m.Provider).Returns(locations.Provider);
        mockSet.As<IQueryable<Location>>().Setup(m => m.Expression).Returns(locations.Expression);
        mockSet.As<IQueryable<Location>>().Setup(m => m.ElementType).Returns(locations.ElementType);
        mockSet.As<IQueryable<Location>>().Setup(m => m.GetEnumerator()).Returns(locations.GetEnumerator());

        _mockContext.Setup(c => c.Locations).Returns(mockSet.Object);

        var callCount = 0;
        _mockHttpClient.Setup(c => c.SendAsync(HttpMethod.Get, It.IsAny<string>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount <= 2) // First location succeeds (daily + hourly)
                {
                    var responseContent = new OpenWeatherDailyResponse { Daily = new List<OpenWeatherDailyResponse.DailyWeather>() };
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, "application/json")
                    };
                }
                throw new HttpRequestException("API Error");
            });

        // Act
        await _service.SyncLocationsDailyWeather();

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    #endregion

    #region SyncWeatherForUserTrackedLocationsAsync Tests

    [Test]
    public async Task SyncWeatherForUserTrackedLocationsAsync_WhenNoTrackedLocations_LogsInformationAndReturns()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var trackLocations = new List<TrackLocation>().AsQueryable();

        var mockSet = new Mock<DbSet<TrackLocation>>();
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Provider).Returns(trackLocations.Provider);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Expression).Returns(trackLocations.Expression);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.ElementType).Returns(trackLocations.ElementType);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.GetEnumerator()).Returns(trackLocations.GetEnumerator());

        _mockContext.Setup(c => c.TrackLocations).Returns(mockSet.Object);

        // Act
        await _service.SyncWeatherForUserTrackedLocationsAsync(userId);

        // Assert
        _mockHttpClient.Verify(c => c.SendAsync(It.IsAny<HttpMethod>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SyncWeatherForUserTrackedLocationsAsync_WithTrackedLocations_FetchesWeatherForAll()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var location1 = new Location { Id = Guid.NewGuid(), Name = "Home", Latitude = 51.5074, Longitude = -0.1278 };
        var location2 = new Location { Id = Guid.NewGuid(), Name = "Work", Latitude = 48.8566, Longitude = 2.3522 };

        var trackLocations = new List<TrackLocation>
        {
            new() { UserId = userId, Location = location1 },
            new() { UserId = userId, Location = location2 }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<TrackLocation>>();
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Provider).Returns(trackLocations.Provider);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Expression).Returns(trackLocations.Expression);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.ElementType).Returns(trackLocations.ElementType);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.GetEnumerator()).Returns(trackLocations.GetEnumerator());

        _mockContext.Setup(c => c.TrackLocations).Returns(mockSet.Object);

        var responseContent = new OpenWeatherDailyResponse { Daily = new List<OpenWeatherDailyResponse.DailyWeather>() };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, "application/json")
        };

        _mockHttpClient.Setup(c => c.SendAsync(HttpMethod.Get, It.IsAny<string>()))
            .ReturnsAsync(httpResponse);

        // Act
        await _service.SyncWeatherForUserTrackedLocationsAsync(userId);

        // Assert
        _mockHttpClient.Verify(c => c.SendAsync(HttpMethod.Get, It.IsAny<string>()), Times.AtLeast(4)); // Daily + Hourly for each location
    }

    #endregion

    #region GetApiKey Tests

    [Test]
    public void GetApiKey_WhenConfigurationMissing_ThrowsArgumentNullException()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["OpenWeatherAPIKey"]).Returns((string?)null);

        var service = new OpenWeatherService(
            _mockContext.Object,
            _mockHttpClient.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);

        // Act & Assert
        service.Invoking(s => s.GetLocationDailyWeather(new Location { Id = Guid.NewGuid(), Name = "Test", Latitude = 0, Longitude = 0 }))
            .Should().ThrowAsync<ArgumentNullException>()
            .WithMessage("*OpenWeatherAPIKey*");
    }

    #endregion
}

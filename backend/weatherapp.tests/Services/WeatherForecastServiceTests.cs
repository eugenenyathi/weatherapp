using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using AutoMapper;
using weatherapp.Data;
using weatherapp.DataTransferObjects;
using weatherapp.Entities;
using weatherapp.Enums;
using weatherapp.Exceptions;
using weatherapp.Services;
using weatherapp.Services.Interfaces;
using NUnit.Framework;

namespace weatherapp.tests.Services;

[TestFixture]
public class WeatherForecastServiceTests
{
    private Mock<AppDbContext> _mockContext = null!;
    private Mock<ILocationService> _mockLocationService = null!;
    private WeatherForecastService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<AppDbContext>();
        _mockLocationService = new Mock<ILocationService>();
        _service = new WeatherForecastService(_mockContext.Object, _mockLocationService.Object);
    }

    #region GetCurrentDaySummariesForAllTrackedLocationsAsync Tests

    [Test]
    public async Task GetCurrentDaySummaries_WhenNoUserPreference_ReturnsSummariesWithMetricUnit()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var trackLocations = new List<TrackLocation>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LocationId = locationId,
                DisplayName = "Home",
                isFavorite = true,
                Location = new Location
                {
                    Id = locationId,
                    Name = "London",
                    DailyWeathers = new List<DayWeather>
                    {
                        new()
                        {
                            Date = today,
                            MinTempMetric = 10,
                            MaxTempMetric = 15,
                            Rain = 5
                        }
                    }
                }
            }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<TrackLocation>>();
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Provider).Returns(trackLocations.Provider);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Expression).Returns(trackLocations.Expression);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.ElementType).Returns(trackLocations.ElementType);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.GetEnumerator()).Returns(trackLocations.GetEnumerator());

        _mockContext.Setup(c => c.TrackLocations).Returns(mockSet.Object);
        _mockContext.Setup(c => c.UserPreferences).Returns(Mock.Of<DbSet<UserPreference>>());
        _mockContext.Setup(c => c.LocationSyncSchedules).Returns(Mock.Of<DbSet<LocationSyncSchedule>>());

        // Act
        var result = await _service.GetCurrentDaySummariesForAllTrackedLocationsAsync(userId);

        // Assert
        result.Should().NotBeEmpty();
        result.First().Unit.Should().Be(Unit.Metric);
        result.First().LocationName.Should().Be("Home");
    }

    [Test]
    public async Task GetCurrentDaySummaries_WithImperialPreference_ReturnsImperialTemperatures()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var userPreferences = new List<UserPreference>
        {
            new() { UserId = userId, PreferredUnit = Unit.Imperial }
        }.AsQueryable();

        var trackLocations = new List<TrackLocation>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LocationId = locationId,
                DisplayName = "Work",
                isFavorite = false,
                Location = new Location
                {
                    Id = locationId,
                    Name = "New York",
                    DailyWeathers = new List<DayWeather>
                    {
                        new()
                        {
                            Date = today,
                            MinTempMetric = 20,
                            MaxTempMetric = 25,
                            MinTempImperial = 68,
                            MaxTempImperial = 77,
                            Rain = 0
                        }
                    }
                }
            }
        }.AsQueryable();

        var mockTrackSet = new Mock<DbSet<TrackLocation>>();
        mockTrackSet.As<IQueryable<TrackLocation>>().Setup(m => m.Provider).Returns(trackLocations.Provider);
        mockTrackSet.As<IQueryable<TrackLocation>>().Setup(m => m.Expression).Returns(trackLocations.Expression);
        mockTrackSet.As<IQueryable<TrackLocation>>().Setup(m => m.ElementType).Returns(trackLocations.ElementType);
        mockTrackSet.As<IQueryable<TrackLocation>>().Setup(m => m.GetEnumerator()).Returns(trackLocations.GetEnumerator());

        var mockPrefSet = new Mock<DbSet<UserPreference>>();
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Provider).Returns(userPreferences.Provider);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Expression).Returns(userPreferences.Expression);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.ElementType).Returns(userPreferences.ElementType);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.GetEnumerator()).Returns(userPreferences.GetEnumerator());
        mockPrefSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<UserPreference, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPreferences.First());

        _mockContext.Setup(c => c.TrackLocations).Returns(mockTrackSet.Object);
        _mockContext.Setup(c => c.UserPreferences).Returns(mockPrefSet.Object);
        _mockContext.Setup(c => c.LocationSyncSchedules).Returns(Mock.Of<DbSet<LocationSyncSchedule>>());

        // Act
        var result = await _service.GetCurrentDaySummariesForAllTrackedLocationsAsync(userId);

        // Assert
        result.Should().NotBeEmpty();
        result.First().Unit.Should().Be(Unit.Imperial);
        result.First().MinTemp.Should().Be(68);
        result.First().MaxTemp.Should().Be(77);
    }

    [Test]
    public async Task GetCurrentDaySummaries_WithNoWeatherData_ReturnsZeroTemperatures()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var trackLocations = new List<TrackLocation>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LocationId = locationId,
                DisplayName = "Test",
                isFavorite = false,
                Location = new Location
                {
                    Id = locationId,
                    Name = "Paris",
                    DailyWeathers = new List<DayWeather>()
                }
            }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<TrackLocation>>();
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Provider).Returns(trackLocations.Provider);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Expression).Returns(trackLocations.Expression);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.ElementType).Returns(trackLocations.ElementType);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.GetEnumerator()).Returns(trackLocations.GetEnumerator());

        _mockContext.Setup(c => c.TrackLocations).Returns(mockSet.Object);
        _mockContext.Setup(c => c.UserPreferences).Returns(Mock.Of<DbSet<UserPreference>>());
        _mockContext.Setup(c => c.LocationSyncSchedules).Returns(Mock.Of<DbSet<LocationSyncSchedule>>());

        // Act
        var result = await _service.GetCurrentDaySummariesForAllTrackedLocationsAsync(userId);

        // Assert
        result.Should().NotBeEmpty();
        result.First().MinTemp.Should().Be(0);
        result.First().MaxTemp.Should().Be(0);
    }

    #endregion

    #region GetFiveDayForecastForLocationAsync Tests

    [Test]
    public async Task GetFiveDayForecast_WhenUserNotTrackingLocation_ThrowsForbiddenException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        _mockContext.Setup(c => c.TrackLocations.AnyAsync(
                It.IsAny<Func<TrackLocation, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await _service.Invoking(s => s.GetFiveDayForecastForLocationAsync(locationId, userId))
            .Should().ThrowAsync<ForbiddenException>()
            .WithMessage("User is not authorized to access this location's forecast.");
    }

    [Test]
    public async Task GetFiveDayForecast_WhenLocationNotFound_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        _mockContext.Setup(c => c.TrackLocations.AnyAsync(
                It.IsAny<Func<TrackLocation, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockContext.Setup(c => c.LocationJobs.AnyAsync(
                It.IsAny<Func<LocationJob, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockContext.Setup(c => c.Locations
                .Where(It.IsAny<Func<Location, bool>>())
                .Include(It.IsAny<Func<Location, IQueryable<Location>>>())
                .FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((Location?)null);

        // Act & Assert
        await _service.Invoking(s => s.GetFiveDayForecastForLocationAsync(locationId, userId))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Location not found.");
    }

    [Test]
    public async Task GetFiveDayForecast_WithPendingJob_WaitsForJobCompletion()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        _mockContext.Setup(c => c.TrackLocations.AnyAsync(
                It.IsAny<Func<TrackLocation, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockContext.Setup(c => c.LocationJobs.AnyAsync(
                It.IsAny<Func<LocationJob, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var location = new Location
        {
            Id = locationId,
            Name = "Tokyo",
            DailyWeathers = new List<DayWeather>
            {
                new() { Date = today, MinTempMetric = 12, MaxTempMetric = 18, Humidity = 65, Summary = "Cloudy" }
            }
        };

        _mockContext.Setup(c => c.Locations
                .Where(It.IsAny<Func<Location, bool>>())
                .Include(It.IsAny<Func<Location, IQueryable<Location>>>())
                .FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        _mockContext.Setup(c => c.LocationSyncSchedules
                .Where(It.IsAny<Func<LocationSyncSchedule, bool>>())
                .MaxAsync(It.IsAny<Func<LocationSyncSchedule, DateTime?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTime.UtcNow);

        // Act
        var result = await _service.GetFiveDayForecastForLocationAsync(locationId, userId);

        // Assert
        _mockLocationService.Verify(s => s.WaitForLocationWeatherDataAsync(locationId, It.IsAny<CancellationToken>()), Times.Once);
        result.LocationName.Should().Be("Tokyo");
        result.FiveDayForecasts.Should().NotBeEmpty();
    }

    [Test]
    public async Task GetFiveDayForecast_WithImperialUnit_ConvertsTemperatures()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var userPreferences = new List<UserPreference>
        {
            new() { UserId = userId, PreferredUnit = Unit.Imperial }
        }.AsQueryable();

        var mockPrefSet = new Mock<DbSet<UserPreference>>();
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Provider).Returns(userPreferences.Provider);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Expression).Returns(userPreferences.Expression);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.ElementType).Returns(userPreferences.ElementType);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.GetEnumerator()).Returns(userPreferences.GetEnumerator());
        mockPrefSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<UserPreference, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPreferences.First());

        _mockContext.Setup(c => c.TrackLocations.AnyAsync(
                It.IsAny<Func<TrackLocation, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockContext.Setup(c => c.UserPreferences).Returns(mockPrefSet.Object);
        _mockContext.Setup(c => c.LocationJobs.AnyAsync(
                It.IsAny<Func<LocationJob, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var location = new Location
        {
            Id = locationId,
            Name = "Berlin",
            DailyWeathers = new List<DayWeather>
            {
                new()
                {
                    Date = today,
                    MinTempMetric = 10,
                    MaxTempMetric = 20,
                    MinTempImperial = 50,
                    MaxTempImperial = 68,
                    Humidity = 50,
                    Summary = "Sunny"
                }
            }
        };

        _mockContext.Setup(c => c.Locations
                .Where(It.IsAny<Func<Location, bool>>())
                .Include(It.IsAny<Func<Location, IQueryable<Location>>>())
                .FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        _mockContext.Setup(c => c.LocationSyncSchedules
                .Where(It.IsAny<Func<LocationSyncSchedule, bool>>())
                .MaxAsync(It.IsAny<Func<LocationSyncSchedule, DateTime?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTime.UtcNow);

        // Act
        var result = await _service.GetFiveDayForecastForLocationAsync(locationId, userId);

        // Assert
        result.Unit.Should().Be(Unit.Imperial);
        result.FiveDayForecasts.First().MinTemp.Should().Be(50);
        result.FiveDayForecasts.First().MaxTemp.Should().Be(68);
    }

    #endregion

    #region GetHourlyForecastForLocationAsync Tests

    [Test]
    public async Task GetHourlyForecast_WhenUserNotTrackingLocation_ThrowsForbiddenException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        _mockContext.Setup(c => c.TrackLocations.AnyAsync(
                It.IsAny<Func<TrackLocation, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await _service.Invoking(s => s.GetHourlyForecastForLocationAsync(locationId, userId))
            .Should().ThrowAsync<ForbiddenException>()
            .WithMessage("User is not authorized to access this location's forecast.");
    }

    [Test]
    public async Task GetHourlyForecast_WhenLocationNotFound_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        _mockContext.Setup(c => c.TrackLocations.AnyAsync(
                It.IsAny<Func<TrackLocation, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockContext.Setup(c => c.LocationJobs.AnyAsync(
                It.IsAny<Func<LocationJob, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockContext.Setup(c => c.Locations
                .Where(It.IsAny<Func<Location, bool>>())
                .Include(It.IsAny<Func<Location, IQueryable<Location>>>())
                .FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((Location?)null);

        // Act & Assert
        await _service.Invoking(s => s.GetHourlyForecastForLocationAsync(locationId, userId))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Location not found.");
    }

    [Test]
    public async Task GetHourlyForecast_WithValidData_ReturnsHourlyForecast()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        _mockContext.Setup(c => c.TrackLocations.AnyAsync(
                It.IsAny<Func<TrackLocation, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockContext.Setup(c => c.LocationJobs.AnyAsync(
                It.IsAny<Func<LocationJob, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var location = new Location
        {
            Id = locationId,
            Name = "Sydney",
            HourlyWeathers = new List<HourWeather>
            {
                new()
                {
                    DateTime = now.AddHours(1),
                    TempMetric = 22,
                    TempImperial = 71.6f,
                    Humidity = 60
                },
                new()
                {
                    DateTime = now.AddHours(2),
                    TempMetric = 23,
                    TempImperial = 73.4f,
                    Humidity = 58
                }
            }
        };

        _mockContext.Setup(c => c.Locations
                .Where(It.IsAny<Func<Location, bool>>())
                .Include(It.IsAny<Func<Location, IQueryable<Location>>>())
                .FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        _mockContext.Setup(c => c.LocationSyncSchedules
                .Where(It.IsAny<Func<LocationSyncSchedule, bool>>())
                .MaxAsync(It.IsAny<Func<LocationSyncSchedule, DateTime?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTime.UtcNow);

        // Act
        var result = await _service.GetHourlyForecastForLocationAsync(locationId, userId);

        // Assert
        result.LocationName.Should().Be("Sydney");
        result.HourlyForecasts.Should().HaveCount(2);
        result.HourlyForecasts.First().TempMetric.Should().Be(22);
    }

    [Test]
    public async Task GetHourlyForecast_WithMetricPreference_ReturnsMetricTemperatures()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var userPreferences = new List<UserPreference>
        {
            new() { UserId = userId, PreferredUnit = Unit.Metric }
        }.AsQueryable();

        var mockPrefSet = new Mock<DbSet<UserPreference>>();
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Provider).Returns(userPreferences.Provider);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Expression).Returns(userPreferences.Expression);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.ElementType).Returns(userPreferences.ElementType);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.GetEnumerator()).Returns(userPreferences.GetEnumerator());
        mockPrefSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<UserPreference, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPreferences.First());

        _mockContext.Setup(c => c.TrackLocations.AnyAsync(
                It.IsAny<Func<TrackLocation, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockContext.Setup(c => c.UserPreferences).Returns(mockPrefSet.Object);
        _mockContext.Setup(c => c.LocationJobs.AnyAsync(
                It.IsAny<Func<LocationJob, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var location = new Location
        {
            Id = locationId,
            Name = "Toronto",
            HourlyWeathers = new List<HourWeather>
            {
                new()
                {
                    DateTime = now.AddHours(1),
                    TempMetric = 15,
                    TempImperial = 59,
                    Humidity = 70
                }
            }
        };

        _mockContext.Setup(c => c.Locations
                .Where(It.IsAny<Func<Location, bool>>())
                .Include(It.IsAny<Func<Location, IQueryable<Location>>>())
                .FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        _mockContext.Setup(c => c.LocationSyncSchedules
                .Where(It.IsAny<Func<LocationSyncSchedule, bool>>())
                .MaxAsync(It.IsAny<Func<LocationSyncSchedule, DateTime?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTime.UtcNow);

        // Act
        var result = await _service.GetHourlyForecastForLocationAsync(locationId, userId);

        // Assert
        result.Unit.Should().Be(Unit.Metric);
        result.HourlyForecasts.First().TempMetric.Should().Be(15);
    }

    #endregion
}

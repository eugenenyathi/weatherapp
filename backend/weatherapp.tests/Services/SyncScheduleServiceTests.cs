using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using weatherapp.Data;
using weatherapp.DataTransferObjects;
using weatherapp.Entities;
using weatherapp.Exceptions;
using weatherapp.Requests;
using weatherapp.Services;
using weatherapp.Services.Interfaces;
using Hangfire;
using NUnit.Framework;

namespace weatherapp.tests.Services;

[TestFixture]
public class SyncScheduleServiceTests
{
    private Mock<AppDbContext> _mockContext = null!;
    private Mock<IRecurringJobManager> _mockRecurringJobManager = null!;
    private Mock<IOpenWeatherService> _mockOpenWeatherService = null!;
    private SyncScheduleService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<AppDbContext>();
        _mockRecurringJobManager = new Mock<IRecurringJobManager>();
        _mockOpenWeatherService = new Mock<IOpenWeatherService>();
        _service = new SyncScheduleService(
            _mockContext.Object,
            _mockRecurringJobManager.Object,
            _mockOpenWeatherService.Object);
    }

    #region InitializeSyncSchedulesForUserAsync Tests

    [Test]
    public async Task InitializeSyncSchedulesForUserAsync_WhenNoTrackedLocations_DoesNothing()
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
        await _service.InitializeSyncSchedulesForUserAsync(userId, 30);

        // Assert
        _mockRecurringJobManager.Verify(m => m.AddOrUpdate(It.IsAny<string>(), It.IsAny<Expression<Func<Task>>>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task InitializeSyncSchedulesForUserAsync_WithTrackedLocations_CreatesRecurringJobs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var trackLocations = new List<TrackLocation>
        {
            new() { UserId = userId, LocationId = locationId }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<TrackLocation>>();
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Provider).Returns(trackLocations.Provider);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Expression).Returns(trackLocations.Expression);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.ElementType).Returns(trackLocations.ElementType);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.GetEnumerator()).Returns(trackLocations.GetEnumerator());

        var syncSchedules = new List<LocationSyncSchedule>().AsQueryable();
        var mockScheduleSet = new Mock<DbSet<LocationSyncSchedule>>();
        mockScheduleSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.Provider).Returns(syncSchedules.Provider);
        mockScheduleSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.Expression).Returns(syncSchedules.Expression);
        mockScheduleSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.ElementType).Returns(syncSchedules.ElementType);
        mockScheduleSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.GetEnumerator()).Returns(syncSchedules.GetEnumerator());
        mockScheduleSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<LocationSyncSchedule, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LocationSyncSchedule?)null);

        _mockContext.Setup(c => c.TrackLocations).Returns(mockSet.Object);
        _mockContext.Setup(c => c.LocationSyncSchedules).Returns(mockScheduleSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _service.InitializeSyncSchedulesForUserAsync(userId, 30);

        // Assert
        _mockRecurringJobManager.Verify(m => m.AddOrUpdate(
            It.IsAny<string>(),
            It.IsAny<Expression<Func<Task>>>(),
            "*/30 * * * *"), Times.Once);
    }

    [Test]
    public async Task InitializeSyncSchedulesForUserAsync_WithMultipleLocations_CreatesJobForEach()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var locationId1 = Guid.NewGuid();
        var locationId2 = Guid.NewGuid();

        var trackLocations = new List<TrackLocation>
        {
            new() { UserId = userId, LocationId = locationId1 },
            new() { UserId = userId, LocationId = locationId2 }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<TrackLocation>>();
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Provider).Returns(trackLocations.Provider);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Expression).Returns(trackLocations.Expression);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.ElementType).Returns(trackLocations.ElementType);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.GetEnumerator()).Returns(trackLocations.GetEnumerator());

        var syncSchedules = new List<LocationSyncSchedule>().AsQueryable();
        var mockScheduleSet = new Mock<DbSet<LocationSyncSchedule>>();
        mockScheduleSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.Provider).Returns(syncSchedules.Provider);
        mockScheduleSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.Expression).Returns(syncSchedules.Expression);
        mockScheduleSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.ElementType).Returns(syncSchedules.ElementType);
        mockScheduleSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.GetEnumerator()).Returns(syncSchedules.GetEnumerator());
        mockScheduleSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<LocationSyncSchedule, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LocationSyncSchedule?)null);

        _mockContext.Setup(c => c.TrackLocations).Returns(mockSet.Object);
        _mockContext.Setup(c => c.LocationSyncSchedules).Returns(mockScheduleSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _service.InitializeSyncSchedulesForUserAsync(userId, 30);

        // Assert
        _mockRecurringJobManager.Verify(m => m.AddOrUpdate(It.IsAny<string>(), It.IsAny<Expression<Func<Task>>>(), It.IsAny<string>()), Times.Exactly(2));
    }

    #endregion

    #region UpdateSyncSchedulesForUserAsync Tests

    [Test]
    public async Task UpdateSyncSchedulesForUserAsync_WhenNoExistingSchedules_InitializesNewSchedules()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var syncSchedules = new List<LocationSyncSchedule>().AsQueryable();

        var mockSet = new Mock<DbSet<LocationSyncSchedule>>();
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.Provider).Returns(syncSchedules.Provider);
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.Expression).Returns(syncSchedules.Expression);
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.ElementType).Returns(syncSchedules.ElementType);
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.GetEnumerator()).Returns(syncSchedules.GetEnumerator());

        _mockContext.Setup(c => c.LocationSyncSchedules).Returns(mockSet.Object);

        // Act
        await _service.UpdateSyncSchedulesForUserAsync(userId, 60);

        // Assert
        _mockRecurringJobManager.Verify(m => m.AddOrUpdate(It.IsAny<string>(), It.IsAny<Expression<Func<Task>>>(), It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task UpdateSyncSchedulesForUserAsync_WithExistingSchedules_UpdatesRecurringJobs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var scheduleId = Guid.NewGuid();

        var existingSchedules = new List<LocationSyncSchedule>
        {
            new()
            {
                Id = scheduleId,
                UserId = userId,
                LocationId = Guid.NewGuid(),
                RecurringJobId = $"sync:{userId.ToString("N").Substring(0, 8)}:job1",
                LastSyncAt = DateTime.MinValue,
                NextSyncAt = DateTime.UtcNow
            }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<LocationSyncSchedule>>();
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.Provider).Returns(existingSchedules.Provider);
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.Expression).Returns(existingSchedules.Expression);
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.ElementType).Returns(existingSchedules.ElementType);
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.GetEnumerator()).Returns(existingSchedules.GetEnumerator());

        _mockContext.Setup(c => c.LocationSyncSchedules).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _service.UpdateSyncSchedulesForUserAsync(userId, 60);

        // Assert
        _mockRecurringJobManager.Verify(m => m.AddOrUpdate(
            It.IsAny<string>(),
            It.IsAny<Expression<Func<Task>>>(),
            "0 */1 * * *"), Times.Once);
    }

    #endregion

    #region RemoveSyncSchedulesForUserAsync Tests

    [Test]
    public async Task RemoveSyncSchedulesForUserAsync_WhenNoSchedules_DoesNothing()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var syncSchedules = new List<LocationSyncSchedule>().AsQueryable();

        var mockSet = new Mock<DbSet<LocationSyncSchedule>>();
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.Provider).Returns(syncSchedules.Provider);
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.Expression).Returns(syncSchedules.Expression);
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.ElementType).Returns(syncSchedules.ElementType);
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.GetEnumerator()).Returns(syncSchedules.GetEnumerator());

        _mockContext.Setup(c => c.LocationSyncSchedules).Returns(mockSet.Object);

        // Act
        await _service.RemoveSyncSchedulesForUserAsync(userId);

        // Assert
        _mockRecurringJobManager.Verify(m => m.RemoveIfExists(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task RemoveSyncSchedulesForUserAsync_WithExistingSchedules_RemovesJobsAndRecords()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var existingSchedules = new List<LocationSyncSchedule>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LocationId = Guid.NewGuid(),
                RecurringJobId = $"sync:{userId.ToString("N").Substring(0, 8)}:job1",
                LastSyncAt = DateTime.MinValue,
                NextSyncAt = DateTime.UtcNow
            }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<LocationSyncSchedule>>();
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.Provider).Returns(existingSchedules.Provider);
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.Expression).Returns(existingSchedules.Expression);
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.ElementType).Returns(existingSchedules.ElementType);
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.GetEnumerator()).Returns(existingSchedules.GetEnumerator());

        _mockContext.Setup(c => c.LocationSyncSchedules).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _service.RemoveSyncSchedulesForUserAsync(userId);

        // Assert
        _mockRecurringJobManager.Verify(m => m.RemoveIfExists(It.IsAny<string>()), Times.Once);
        _mockContext.Verify(c => c.LocationSyncSchedules.RemoveRange(It.IsAny<IEnumerable<LocationSyncSchedule>>()), Times.Once);
    }

    #endregion

    #region TriggerSyncForUserAsync Tests

    [Test]
    public async Task TriggerSyncForUserAsync_EnqueuesImmediateSync()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        await _service.TriggerSyncForUserAsync(userId);

        // Assert
        _mockOpenWeatherService.Verify(s => s.SyncWeatherForUserTrackedLocationsAsync(userId), Times.Once);
    }

    #endregion

    #region RefreshWeatherForUserTrackedLocationsAsync Tests

    [Test]
    public async Task RefreshWeatherForUserTrackedLocationsAsync_WhenPreferenceNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var preferences = new List<UserPreference>().AsQueryable();

        var mockSet = new Mock<DbSet<UserPreference>>();
        mockSet.As<IQueryable<UserPreference>>().Setup(m => m.Provider).Returns(preferences.Provider);
        mockSet.As<IQueryable<UserPreference>>().Setup(m => m.Expression).Returns(preferences.Expression);
        mockSet.As<IQueryable<UserPreference>>().Setup(m => m.ElementType).Returns(preferences.ElementType);
        mockSet.As<IQueryable<UserPreference>>().Setup(m => m.GetEnumerator()).Returns(preferences.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<UserPreference, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserPreference?)null);

        _mockContext.Setup(c => c.UserPreferences).Returns(mockSet.Object);

        // Act & Assert
        await _service.Invoking(s => s.RefreshWeatherForUserTrackedLocationsAsync(userId))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("User preference not found. Please set up your preferences first.");
    }

    [Test]
    public async Task RefreshWeatherForUserTrackedLocationsAsync_WhenRateLimitExceeded_ThrowsRateLimitExceededException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var userPreference = new UserPreference
        {
            UserId = userId,
            LastManualRefreshAt = DateTime.UtcNow.AddMinutes(-5) // Refreshed 5 minutes ago
        };

        var preferences = new List<UserPreference> { userPreference }.AsQueryable();

        var mockSet = new Mock<DbSet<UserPreference>>();
        mockSet.As<IQueryable<UserPreference>>().Setup(m => m.Provider).Returns(preferences.Provider);
        mockSet.As<IQueryable<UserPreference>>().Setup(m => m.Expression).Returns(preferences.Expression);
        mockSet.As<IQueryable<UserPreference>>().Setup(m => m.ElementType).Returns(preferences.ElementType);
        mockSet.As<IQueryable<UserPreference>>().Setup(m => m.GetEnumerator()).Returns(preferences.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<UserPreference, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPreference);

        _mockContext.Setup(c => c.UserPreferences).Returns(mockSet.Object);

        // Act & Assert
        await _service.Invoking(s => s.RefreshWeatherForUserTrackedLocationsAsync(userId))
            .Should().ThrowAsync<RateLimitExceededException>();
    }

    [Test]
    public async Task RefreshWeatherForUserTrackedLocationsAsync_WhenWithinRateLimit_RefreshesSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var userPreference = new UserPreference
        {
            UserId = userId,
            LastManualRefreshAt = DateTime.UtcNow.AddMinutes(-20) // Refreshed 20 minutes ago
        };

        var preferences = new List<UserPreference> { userPreference }.AsQueryable();

        var syncSchedules = new List<LocationSyncSchedule>
        {
            new() { UserId = userId, LastSyncAt = DateTime.UtcNow }
        }.AsQueryable();

        var mockPrefSet = new Mock<DbSet<UserPreference>>();
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Provider).Returns(preferences.Provider);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Expression).Returns(preferences.Expression);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.ElementType).Returns(preferences.ElementType);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.GetEnumerator()).Returns(preferences.GetEnumerator());
        mockPrefSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<UserPreference, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPreference);

        var mockScheduleSet = new Mock<DbSet<LocationSyncSchedule>>();
        mockScheduleSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.Provider).Returns(syncSchedules.Provider);
        mockScheduleSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.Expression).Returns(syncSchedules.Expression);
        mockScheduleSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.ElementType).Returns(syncSchedules.ElementType);
        mockScheduleSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.GetEnumerator()).Returns(syncSchedules.GetEnumerator());
        mockScheduleSet.Setup(m => m.MaxAsync(It.IsAny<Func<LocationSyncSchedule, DateTime?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTime.UtcNow);

        _mockContext.Setup(c => c.UserPreferences).Returns(mockPrefSet.Object);
        _mockContext.Setup(c => c.LocationSyncSchedules).Returns(mockScheduleSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.RefreshWeatherForUserTrackedLocationsAsync(userId);

        // Assert
        result.Success.Should().BeTrue();
        result.LastSyncedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region GetCronFromMinutes Tests (via UpdateSyncSchedulesForUserAsync)

    [Test]
    public async Task UpdateSyncSchedulesForUserAsync_With30Minutes_UsesCorrectCronExpression()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var existingSchedules = new List<LocationSyncSchedule>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LocationId = Guid.NewGuid(),
                RecurringJobId = "test-job",
                LastSyncAt = DateTime.MinValue,
                NextSyncAt = DateTime.UtcNow
            }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<LocationSyncSchedule>>();
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.Provider).Returns(existingSchedules.Provider);
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.Expression).Returns(existingSchedules.Expression);
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.ElementType).Returns(existingSchedules.ElementType);
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.GetEnumerator()).Returns(existingSchedules.GetEnumerator());

        _mockContext.Setup(c => c.LocationSyncSchedules).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _service.UpdateSyncSchedulesForUserAsync(userId, 30);

        // Assert
        _mockRecurringJobManager.Verify(m => m.AddOrUpdate(
            It.IsAny<string>(),
            It.IsAny<Expression<Func<Task>>>(),
            "*/30 * * * *"), Times.Once);
    }

    [Test]
    public async Task UpdateSyncSchedulesForUserAsync_With60Minutes_UsesHourlyCronExpression()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var existingSchedules = new List<LocationSyncSchedule>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LocationId = Guid.NewGuid(),
                RecurringJobId = "test-job",
                LastSyncAt = DateTime.MinValue,
                NextSyncAt = DateTime.UtcNow
            }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<LocationSyncSchedule>>();
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.Provider).Returns(existingSchedules.Provider);
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.Expression).Returns(existingSchedules.Expression);
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.ElementType).Returns(existingSchedules.ElementType);
        mockSet.As<IQueryable<LocationSyncSchedule>>().Setup(m => m.GetEnumerator()).Returns(existingSchedules.GetEnumerator());

        _mockContext.Setup(c => c.LocationSyncSchedules).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _service.UpdateSyncSchedulesForUserAsync(userId, 60);

        // Assert
        _mockRecurringJobManager.Verify(m => m.AddOrUpdate(
            It.IsAny<string>(),
            It.IsAny<Expression<Func<Task>>>(),
            "0 */1 * * *"), Times.Once);
    }

    #endregion
}

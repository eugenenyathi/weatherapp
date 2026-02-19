using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using AutoMapper;
using weatherapp.Data;
using weatherapp.DataTransferObjects;
using weatherapp.Entities;
using weatherapp.Enums;
using weatherapp.Exceptions;
using weatherapp.Requests;
using weatherapp.Services;
using weatherapp.Services.Interfaces;
using NUnit.Framework;

namespace weatherapp.tests.Services;

[TestFixture]
public class UserPreferenceServiceTests
{
    private Mock<AppDbContext> _mockContext = null!;
    private Mock<IMapper> _mockMapper = null!;
    private Mock<ISyncScheduleService> _mockSyncScheduleService = null!;
    private UserPreferenceService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<AppDbContext>();
        _mockMapper = new Mock<IMapper>();
        _mockSyncScheduleService = new Mock<ISyncScheduleService>();
        _service = new UserPreferenceService(_mockContext.Object, _mockMapper.Object, _mockSyncScheduleService.Object);
    }

    #region GetByUserIdAsync Tests

    [Test]
    public async Task GetByUserIdAsync_WhenPreferenceExists_ReturnsUserPreferenceDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferenceId = Guid.NewGuid();
        
        var userPreference = new UserPreference
        {
            Id = preferenceId,
            UserId = userId,
            PreferredUnit = Unit.Metric,
            RefreshInterval = 30
        };

        var userPreferenceDto = new UserPreferenceDto
        {
            Id = preferenceId,
            UserId = userId,
            PreferredUnit = Unit.Metric,
            RefreshInterval = 30
        };

        var userPreferences = new List<UserPreference> { userPreference }.AsQueryable();

        var mockSet = new Mock<DbSet<UserPreference>>();
        mockSet.As<IQueryable<UserPreference>>().Setup(m => m.Provider).Returns(userPreferences.Provider);
        mockSet.As<IQueryable<UserPreference>>().Setup(m => m.Expression).Returns(userPreferences.Expression);
        mockSet.As<IQueryable<UserPreference>>().Setup(m => m.ElementType).Returns(userPreferences.ElementType);
        mockSet.As<IQueryable<UserPreference>>().Setup(m => m.GetEnumerator()).Returns(userPreferences.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<UserPreference, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPreference);

        _mockContext.Setup(c => c.UserPreferences).Returns(mockSet.Object);
        _mockMapper.Setup(m => m.Map<UserPreferenceDto>(userPreference)).Returns(userPreferenceDto);

        // Act
        var result = await _service.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(preferenceId);
        result.PreferredUnit.Should().Be(Unit.Metric);
    }

    [Test]
    public async Task GetByUserIdAsync_WhenPreferenceDoesNotExist_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var userPreferences = new List<UserPreference>().AsQueryable();

        var mockSet = new Mock<DbSet<UserPreference>>();
        mockSet.As<IQueryable<UserPreference>>().Setup(m => m.Provider).Returns(userPreferences.Provider);
        mockSet.As<IQueryable<UserPreference>>().Setup(m => m.Expression).Returns(userPreferences.Expression);
        mockSet.As<IQueryable<UserPreference>>().Setup(m => m.ElementType).Returns(userPreferences.ElementType);
        mockSet.As<IQueryable<UserPreference>>().Setup(m => m.GetEnumerator()).Returns(userPreferences.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<UserPreference, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserPreference?)null);

        _mockContext.Setup(c => c.UserPreferences).Returns(mockSet.Object);

        // Act
        var result = await _service.GetByUserIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateAsync Tests

    [Test]
    public async Task CreateAsync_WhenUserDoesNotExist_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UserPreferenceRequest { PreferredUnit = Unit.Metric, RefreshInterval = 30 };

        var users = new List<User>().AsQueryable();

        var mockUserSet = new Mock<DbSet<User>>();
        mockUserSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        mockUserSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        mockUserSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        mockUserSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());
        mockUserSet.Setup(m => m.AnyAsync(It.IsAny<Func<User, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockContext.Setup(c => c.Users).Returns(mockUserSet.Object);

        // Act & Assert
        await _service.Invoking(s => s.CreateAsync(userId, request))
            .Should().ThrowAsync<Exception>()
            .WithMessage("User doesn't exist");
    }

    [Test]
    public async Task CreateAsync_WhenPreferenceAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UserPreferenceRequest { PreferredUnit = Unit.Metric, RefreshInterval = 30 };

        var users = new List<User> { new User { Id = userId } }.AsQueryable();
        var existingPreferences = new List<UserPreference>
        {
            new() { Id = Guid.NewGuid(), UserId = userId }
        }.AsQueryable();

        var mockUserSet = new Mock<DbSet<User>>();
        mockUserSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        mockUserSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        mockUserSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        mockUserSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());
        mockUserSet.Setup(m => m.AnyAsync(It.IsAny<Func<User, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var mockPrefSet = new Mock<DbSet<UserPreference>>();
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Provider).Returns(existingPreferences.Provider);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Expression).Returns(existingPreferences.Expression);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.ElementType).Returns(existingPreferences.ElementType);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.GetEnumerator()).Returns(existingPreferences.GetEnumerator());
        mockPrefSet.Setup(m => m.AnyAsync(It.IsAny<Func<UserPreference, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockContext.Setup(c => c.Users).Returns(mockUserSet.Object);
        _mockContext.Setup(c => c.UserPreferences).Returns(mockPrefSet.Object);

        // Act & Assert
        await _service.Invoking(s => s.CreateAsync(userId, request))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"User preference already exists for user ID {userId}.");
    }

    [Test]
    public async Task CreateAsync_WithValidRequest_CreatesPreferenceAndInitializesSyncSchedules()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferenceId = Guid.NewGuid();
        var request = new UserPreferenceRequest { PreferredUnit = Unit.Metric, RefreshInterval = 45 };

        var newUserPreference = new UserPreference
        {
            Id = preferenceId,
            UserId = userId,
            PreferredUnit = Unit.Metric,
            RefreshInterval = 45
        };

        var userPreferenceDto = new UserPreferenceDto
        {
            Id = preferenceId,
            UserId = userId,
            PreferredUnit = Unit.Metric,
            RefreshInterval = 45
        };

        var users = new List<User> { new User { Id = userId } }.AsQueryable();
        var preferences = new List<UserPreference>().AsQueryable();

        var mockUserSet = new Mock<DbSet<User>>();
        mockUserSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        mockUserSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        mockUserSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        mockUserSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());
        mockUserSet.Setup(m => m.AnyAsync(It.IsAny<Func<User, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var mockPrefSet = new Mock<DbSet<UserPreference>>();
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Provider).Returns(preferences.Provider);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Expression).Returns(preferences.Expression);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.ElementType).Returns(preferences.ElementType);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.GetEnumerator()).Returns(preferences.GetEnumerator());
        mockPrefSet.Setup(m => m.AnyAsync(It.IsAny<Func<UserPreference, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockContext.Setup(c => c.Users).Returns(mockUserSet.Object);
        _mockContext.Setup(c => c.UserPreferences).Returns(mockPrefSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<UserPreferenceDto>(It.IsAny<UserPreference>())).Returns(userPreferenceDto);

        // Act
        var result = await _service.CreateAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.PreferredUnit.Should().Be(Unit.Metric);
        result.RefreshInterval.Should().Be(45);
        _mockSyncScheduleService.Verify(s => s.InitializeSyncSchedulesForUserAsync(userId, 45), Times.Once);
    }

    [Test]
    public async Task CreateAsync_WithoutPreferredUnit_UsesMetricDefault()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferenceId = Guid.NewGuid();
        var request = new UserPreferenceRequest { RefreshInterval = 60 };

        var newUserPreference = new UserPreference
        {
            Id = preferenceId,
            UserId = userId,
            PreferredUnit = Unit.Metric,
            RefreshInterval = 60
        };

        var userPreferenceDto = new UserPreferenceDto
        {
            Id = preferenceId,
            UserId = userId,
            PreferredUnit = Unit.Metric,
            RefreshInterval = 60
        };

        var users = new List<User> { new User { Id = userId } }.AsQueryable();
        var preferences = new List<UserPreference>().AsQueryable();

        var mockUserSet = new Mock<DbSet<User>>();
        mockUserSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        mockUserSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        mockUserSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        mockUserSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());
        mockUserSet.Setup(m => m.AnyAsync(It.IsAny<Func<User, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var mockPrefSet = new Mock<DbSet<UserPreference>>();
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Provider).Returns(preferences.Provider);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Expression).Returns(preferences.Expression);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.ElementType).Returns(preferences.ElementType);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.GetEnumerator()).Returns(preferences.GetEnumerator());
        mockPrefSet.Setup(m => m.AnyAsync(It.IsAny<Func<UserPreference, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockContext.Setup(c => c.Users).Returns(mockUserSet.Object);
        _mockContext.Setup(c => c.UserPreferences).Returns(mockPrefSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<UserPreferenceDto>(It.IsAny<UserPreference>())).Returns(userPreferenceDto);

        // Act
        var result = await _service.CreateAsync(userId, request);

        // Assert
        result.PreferredUnit.Should().Be(Unit.Metric);
    }

    #endregion

    #region UpdateAsync Tests

    [Test]
    public async Task UpdateAsync_WhenPreferenceNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferenceId = Guid.NewGuid();
        var request = new UserPreferenceRequest { PreferredUnit = Unit.Imperial };

        var preferences = new List<UserPreference>().AsQueryable();

        var mockPrefSet = new Mock<DbSet<UserPreference>>();
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Provider).Returns(preferences.Provider);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Expression).Returns(preferences.Expression);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.ElementType).Returns(preferences.ElementType);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.GetEnumerator()).Returns(preferences.GetEnumerator());
        mockPrefSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<UserPreference, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserPreference?)null);

        _mockContext.Setup(c => c.UserPreferences).Returns(mockPrefSet.Object);

        // Act & Assert
        await _service.Invoking(s => s.UpdateAsync(userId, preferenceId, request))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage($"User preference not found for user ID {userId} and preference ID {preferenceId}.");
    }

    [Test]
    public async Task UpdateAsync_WithValidRequest_UpdatesPreference()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferenceId = Guid.NewGuid();
        var request = new UserPreferenceRequest { PreferredUnit = Unit.Imperial, RefreshInterval = 60 };

        var existingPreference = new UserPreference
        {
            Id = preferenceId,
            UserId = userId,
            PreferredUnit = Unit.Metric,
            RefreshInterval = 30
        };

        var updatedPreferenceDto = new UserPreferenceDto
        {
            Id = preferenceId,
            UserId = userId,
            PreferredUnit = Unit.Imperial,
            RefreshInterval = 60
        };

        var preferences = new List<UserPreference> { existingPreference }.AsQueryable();

        var mockPrefSet = new Mock<DbSet<UserPreference>>();
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Provider).Returns(preferences.Provider);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Expression).Returns(preferences.Expression);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.ElementType).Returns(preferences.ElementType);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.GetEnumerator()).Returns(preferences.GetEnumerator());
        mockPrefSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<UserPreference, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPreference);

        _mockContext.Setup(c => c.UserPreferences).Returns(mockPrefSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<UserPreferenceDto>(existingPreference)).Returns(updatedPreferenceDto);

        // Act
        var result = await _service.UpdateAsync(userId, preferenceId, request);

        // Assert
        result.Should().NotBeNull();
        result.PreferredUnit.Should().Be(Unit.Imperial);
        result.RefreshInterval.Should().Be(60);
        existingPreference.PreferredUnit.Should().Be(Unit.Imperial);
        existingPreference.RefreshInterval.Should().Be(60);
    }

    [Test]
    public async Task UpdateAsync_WhenRefreshIntervalChanges_UpdatesSyncSchedules()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferenceId = Guid.NewGuid();
        var request = new UserPreferenceRequest { RefreshInterval = 90 };

        var existingPreference = new UserPreference
        {
            Id = preferenceId,
            UserId = userId,
            PreferredUnit = Unit.Metric,
            RefreshInterval = 30
        };

        var updatedPreferenceDto = new UserPreferenceDto
        {
            Id = preferenceId,
            UserId = userId,
            PreferredUnit = Unit.Metric,
            RefreshInterval = 90
        };

        var preferences = new List<UserPreference> { existingPreference }.AsQueryable();

        var mockPrefSet = new Mock<DbSet<UserPreference>>();
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Provider).Returns(preferences.Provider);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Expression).Returns(preferences.Expression);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.ElementType).Returns(preferences.ElementType);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.GetEnumerator()).Returns(preferences.GetEnumerator());
        mockPrefSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<UserPreference, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPreference);

        _mockContext.Setup(c => c.UserPreferences).Returns(mockPrefSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<UserPreferenceDto>(existingPreference)).Returns(updatedPreferenceDto);

        // Act
        var result = await _service.UpdateAsync(userId, preferenceId, request);

        // Assert
        _mockSyncScheduleService.Verify(s => s.UpdateSyncSchedulesForUserAsync(userId, 90), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_WhenRefreshIntervalUnchanged_DoesNotUpdateSyncSchedules()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferenceId = Guid.NewGuid();
        var request = new UserPreferenceRequest { PreferredUnit = Unit.Imperial };

        var existingPreference = new UserPreference
        {
            Id = preferenceId,
            UserId = userId,
            PreferredUnit = Unit.Metric,
            RefreshInterval = 30
        };

        var updatedPreferenceDto = new UserPreferenceDto
        {
            Id = preferenceId,
            UserId = userId,
            PreferredUnit = Unit.Imperial,
            RefreshInterval = 30
        };

        var preferences = new List<UserPreference> { existingPreference }.AsQueryable();

        var mockPrefSet = new Mock<DbSet<UserPreference>>();
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Provider).Returns(preferences.Provider);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Expression).Returns(preferences.Expression);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.ElementType).Returns(preferences.ElementType);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.GetEnumerator()).Returns(preferences.GetEnumerator());
        mockPrefSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<UserPreference, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPreference);

        _mockContext.Setup(c => c.UserPreferences).Returns(mockPrefSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<UserPreferenceDto>(existingPreference)).Returns(updatedPreferenceDto);

        // Act
        var result = await _service.UpdateAsync(userId, preferenceId, request);

        // Assert
        _mockSyncScheduleService.Verify(s => s.UpdateSyncSchedulesForUserAsync(It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region DeleteAsync Tests

    [Test]
    public async Task DeleteAsync_WhenPreferenceNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferenceId = Guid.NewGuid();

        var preferences = new List<UserPreference>().AsQueryable();

        var mockPrefSet = new Mock<DbSet<UserPreference>>();
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Provider).Returns(preferences.Provider);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Expression).Returns(preferences.Expression);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.ElementType).Returns(preferences.ElementType);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.GetEnumerator()).Returns(preferences.GetEnumerator());
        mockPrefSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<UserPreference, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserPreference?)null);

        _mockContext.Setup(c => c.UserPreferences).Returns(mockPrefSet.Object);

        // Act & Assert
        await _service.Invoking(s => s.DeleteAsync(userId, preferenceId))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Failed to delete User Preference with ID {preferenceId} for user {userId}");
    }

    [Test]
    public async Task DeleteAsync_WithValidData_RemovesPreferenceAndSyncSchedules()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferenceId = Guid.NewGuid();

        var existingPreference = new UserPreference
        {
            Id = preferenceId,
            UserId = userId
        };

        var preferences = new List<UserPreference> { existingPreference }.AsQueryable();

        var mockPrefSet = new Mock<DbSet<UserPreference>>();
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Provider).Returns(preferences.Provider);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.Expression).Returns(preferences.Expression);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.ElementType).Returns(preferences.ElementType);
        mockPrefSet.As<IQueryable<UserPreference>>().Setup(m => m.GetEnumerator()).Returns(preferences.GetEnumerator());
        mockPrefSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<UserPreference, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPreference);

        _mockContext.Setup(c => c.UserPreferences).Returns(mockPrefSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _service.DeleteAsync(userId, preferenceId);

        // Assert
        _mockSyncScheduleService.Verify(s => s.RemoveSyncSchedulesForUserAsync(userId), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}

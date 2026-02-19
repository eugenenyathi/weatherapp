using FluentAssertions;
using Moq;
using weatherapp.Services;
using weatherapp.Services.Interfaces;
using Hangfire;
using NUnit.Framework;

namespace weatherapp.tests.Services;

[TestFixture]
public class GlobalSyncServiceTests
{
    private Mock<IRecurringJobManager> _mockRecurringJobManager = null!;
    private Mock<IOpenWeatherService> _mockOpenWeatherService = null!;
    private GlobalSyncService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockRecurringJobManager = new Mock<IRecurringJobManager>();
        _mockOpenWeatherService = new Mock<IOpenWeatherService>();
        _service = new GlobalSyncService(_mockRecurringJobManager.Object, _mockOpenWeatherService.Object);
    }

    #region InitializeGlobalSyncAsync Tests

    [Test]
    public async Task InitializeGlobalSyncAsync_RegistersRecurringJobWithHourlySchedule()
    {
        // Arrange
        const string expectedJobId = "global-locations-weather-sync";
        const string expectedCron = "0 * * * *"; // Every hour at minute 0

        // Act
        await _service.InitializeGlobalSyncAsync();

        // Assert
        _mockRecurringJobManager.Verify(m => m.AddOrUpdate(
            expectedJobId,
            It.IsAny<Expression<Func<Task>>>(),
            expectedCron), Times.Once);
    }

    [Test]
    public async Task InitializeGlobalSyncAsync_RegistersJobThatCallsSyncLocationsDailyWeather()
    {
        // Arrange
        const string expectedJobId = "global-locations-weather-sync";

        // Act
        await _service.InitializeGlobalSyncAsync();

        // Assert
        _mockRecurringJobManager.Verify(m => m.AddOrUpdate(
            expectedJobId,
            It.Is<Expression<Func<Task>>>(expr => 
            {
                // Verify the expression calls SyncLocationsDailyWeather
                var body = (MethodCallExpression)((LambdaExpression)expr).Body;
                return body.Method.Name == "SyncLocationsDailyWeather";
            }),
            It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task InitializeGlobalSyncAsync_ReturnsCompletedTask()
    {
        // Act
        var result = _service.InitializeGlobalSyncAsync();

        // Assert
        result.Should().NotBeNull();
        await result.Should().CompleteWithinAsync(TimeSpan.FromSeconds(1));
    }

    #endregion

    #region RemoveGlobalSyncAsync Tests

    [Test]
    public async Task RemoveGlobalSyncAsync_RemovesRecurringJob()
    {
        // Arrange
        const string expectedJobId = "global-locations-weather-sync";

        // Act
        await _service.RemoveGlobalSyncAsync();

        // Assert
        _mockRecurringJobManager.Verify(m => m.RemoveIfExists(expectedJobId), Times.Once);
    }

    [Test]
    public async Task RemoveGlobalSyncAsync_ReturnsCompletedTask()
    {
        // Act
        var result = _service.RemoveGlobalSyncAsync();

        // Assert
        result.Should().NotBeNull();
        await result.Should().CompleteWithinAsync(TimeSpan.FromSeconds(1));
    }

    #endregion

    #region GlobalSyncJobId Constant Tests

    [Test]
    public void GlobalSyncJobId_HasExpectedValue()
    {
        // This test verifies the constant value used for the job ID
        // Using reflection to access the private constant
        var type = typeof(GlobalSyncService);
        const string expectedJobId = "global-locations-weather-sync";
        
        // The job ID is hardcoded in the methods, so we verify it's used correctly
        _mockRecurringJobManager.Verify(m => m.AddOrUpdate(
            It.Is<string>(id => id == expectedJobId),
            It.IsAny<Expression<Func<Task>>>(),
            It.IsAny<string>()), Times.Never); // Not called yet

        // Act
        _service.InitializeGlobalSyncAsync().Wait();

        // Assert
        _mockRecurringJobManager.Verify(m => m.AddOrUpdate(
            expectedJobId,
            It.IsAny<Expression<Func<Task>>>(),
            It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region Multiple Initialization Tests

    [Test]
    public async Task InitializeGlobalSyncAsync_MultipleCalls_UpdatesExistingJob()
    {
        // Arrange & Act
        await _service.InitializeGlobalSyncAsync();
        await _service.InitializeGlobalSyncAsync();

        // Assert - AddOrUpdate should be called twice (once per initialization)
        _mockRecurringJobManager.Verify(m => m.AddOrUpdate(
            It.IsAny<string>(),
            It.IsAny<Expression<Func<Task>>>(),
            It.IsAny<string>()), Times.Exactly(2));
    }

    #endregion

    #region Remove Before Initialize Tests

    [Test]
    public async Task RemoveGlobalSyncAsync_BeforeInitialize_RemovesNonExistentJob()
    {
        // Act
        await _service.RemoveGlobalSyncAsync();

        // Assert - Should still call RemoveIfExists even if job doesn't exist
        _mockRecurringJobManager.Verify(m => m.RemoveIfExists("global-locations-weather-sync"), Times.Once);
    }

    [Test]
    public async Task InitializeGlobalSyncAsync_AfterRemove_ReRegistersJob()
    {
        // Arrange
        await _service.RemoveGlobalSyncAsync();

        // Act
        await _service.InitializeGlobalSyncAsync();

        // Assert
        _mockRecurringJobManager.Verify(m => m.AddOrUpdate(
            "global-locations-weather-sync",
            It.IsAny<Expression<Func<Task>>>(),
            It.IsAny<string>()), Times.Once);
    }

    #endregion
}

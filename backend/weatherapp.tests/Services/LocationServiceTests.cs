using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using AutoMapper;
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
public class LocationServiceTests
{
    private Mock<AppDbContext> _mockContext = null!;
    private Mock<IMapper> _mockMapper = null!;
    private Mock<IBackgroundJobClient> _mockBackgroundJobClient = null!;
    private Mock<IOpenWeatherService> _mockOpenWeatherService = null!;
    private LocationService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<AppDbContext>();
        _mockMapper = new Mock<IMapper>();
        _mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
        _mockOpenWeatherService = new Mock<IOpenWeatherService>();
        _service = new LocationService(
            _mockContext.Object,
            _mockMapper.Object,
            _mockBackgroundJobClient.Object,
            _mockOpenWeatherService.Object);
    }

    #region GetAllAsync Tests

    [Test]
    public async Task GetAllAsync_WhenLocationsExist_ReturnsAllLocations()
    {
        // Arrange
        var locations = new List<Location>
        {
            new() { Id = Guid.NewGuid(), Name = "London", Latitude = 51.5074, Longitude = -0.1278, Country = "UK" },
            new() { Id = Guid.NewGuid(), Name = "Paris", Latitude = 48.8566, Longitude = 2.3522, Country = "France" }
        }.AsQueryable();

        var locationDtos = new List<LocationDto>
        {
            new() { Id = locations.First().Id, Name = "London" },
            new() { Id = locations.Last().Id, Name = "Paris" }
        };

        var mockSet = new Mock<DbSet<Location>>();
        mockSet.As<IQueryable<Location>>().Setup(m => m.Provider).Returns(locations.Provider);
        mockSet.As<IQueryable<Location>>().Setup(m => m.Expression).Returns(locations.Expression);
        mockSet.As<IQueryable<Location>>().Setup(m => m.ElementType).Returns(locations.ElementType);
        mockSet.As<IQueryable<Location>>().Setup(m => m.GetEnumerator()).Returns(locations.GetEnumerator());

        _mockContext.Setup(c => c.Locations).Returns(mockSet.Object);
        _mockMapper.Setup(m => m.ProjectTo<LocationDto>(It.IsAny<object>()))
            .Returns(locationDtos.AsQueryable());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(l => l.Name == "London");
        result.Should().Contain(l => l.Name == "Paris");
    }

    [Test]
    public async Task GetAllAsync_WhenNoLocations_ReturnsEmptyList()
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
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateAsync Tests

    [Test]
    public async Task CreateAsync_WithValidRequest_CreatesLocationAndEnqueuesJobs()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var request = new LocationRequest
        {
            Name = "Tokyo",
            Latitude = 35.6762,
            Longitude = 139.6503,
            Country = "Japan"
        };

        var newLocation = new Location
        {
            Id = locationId,
            Name = request.Name,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Country = request.Country
        };

        var locationDto = new LocationDto
        {
            Id = locationId,
            Name = request.Name
        };

        var locations = new List<Location>().AsQueryable();

        var mockSet = new Mock<DbSet<Location>>();
        mockSet.As<IQueryable<Location>>().Setup(m => m.Provider).Returns(locations.Provider);
        mockSet.As<IQueryable<Location>>().Setup(m => m.Expression).Returns(locations.Expression);
        mockSet.As<IQueryable<Location>>().Setup(m => m.ElementType).Returns(locations.ElementType);
        mockSet.As<IQueryable<Location>>().Setup(m => m.GetEnumerator()).Returns(locations.GetEnumerator());
        mockSet.Setup(m => m.AnyAsync(It.IsAny<Func<Location, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockContext.Setup(c => c.Locations).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<Location>(request)).Returns(newLocation);
        _mockMapper.Setup(m => m.Map<LocationDto>(newLocation)).Returns(locationDto);
        _mockBackgroundJobClient.Setup(c => c.Enqueue(It.IsAny<Expression<Func<Task>>>(), It.IsAny<TimeSpan?>()))
            .Returns("job-id-1");

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        _mockContext.Verify(c => c.Locations.AddAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockBackgroundJobClient.Verify(c => c.Enqueue(It.IsAny<Expression<Func<Task>>>(), It.IsAny<TimeSpan?>()), Times.Exactly(2));
    }

    [Test]
    public async Task CreateAsync_WithDuplicateName_ThrowsDuplicateLocationException()
    {
        // Arrange
        var request = new LocationRequest
        {
            Name = "Existing City",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Country = "USA"
        };

        var locations = new List<Location>
        {
            new() { Name = request.Name }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<Location>>();
        mockSet.As<IQueryable<Location>>().Setup(m => m.Provider).Returns(locations.Provider);
        mockSet.As<IQueryable<Location>>().Setup(m => m.Expression).Returns(locations.Expression);
        mockSet.As<IQueryable<Location>>().Setup(m => m.ElementType).Returns(locations.ElementType);
        mockSet.As<IQueryable<Location>>().Setup(m => m.GetEnumerator()).Returns(locations.GetEnumerator());
        mockSet.Setup(m => m.AnyAsync(It.IsAny<Func<Location, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockContext.Setup(c => c.Locations).Returns(mockSet.Object);

        // Act & Assert
        await _service.Invoking(s => s.CreateAsync(request))
            .Should().ThrowAsync<DuplicateLocationException>()
            .WithMessage($"Location with name '{request.Name}' already exists.");
    }

    [Test]
    public async Task CreateAsync_CreatesLocationJobEntity()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var request = new LocationRequest
        {
            Name = "New Location",
            Latitude = 35.6762,
            Longitude = 139.6503,
            Country = "Japan"
        };

        var newLocation = new Location
        {
            Id = locationId,
            Name = request.Name
        };

        var locationDto = new LocationDto
        {
            Id = locationId,
            Name = request.Name
        };

        var locations = new List<Location>().AsQueryable();

        var mockSet = new Mock<DbSet<Location>>();
        mockSet.As<IQueryable<Location>>().Setup(m => m.Provider).Returns(locations.Provider);
        mockSet.As<IQueryable<Location>>().Setup(m => m.Expression).Returns(locations.Expression);
        mockSet.As<IQueryable<Location>>().Setup(m => m.ElementType).Returns(locations.ElementType);
        mockSet.As<IQueryable<Location>>().Setup(m => m.GetEnumerator()).Returns(locations.GetEnumerator());
        mockSet.Setup(m => m.AnyAsync(It.IsAny<Func<Location, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var locationJobs = new List<LocationJob>().AsQueryable();
        var mockJobSet = new Mock<DbSet<LocationJob>>();
        mockJobSet.As<IQueryable<LocationJob>>().Setup(m => m.Provider).Returns(locationJobs.Provider);
        mockJobSet.As<IQueryable<LocationJob>>().Setup(m => m.Expression).Returns(locationJobs.Expression);
        mockJobSet.As<IQueryable<LocationJob>>().Setup(m => m.ElementType).Returns(locationJobs.ElementType);
        mockJobSet.As<IQueryable<LocationJob>>().Setup(m => m.GetEnumerator()).Returns(locationJobs.GetEnumerator());

        _mockContext.Setup(c => c.Locations).Returns(mockSet.Object);
        _mockContext.Setup(c => c.LocationJobs).Returns(mockJobSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<Location>(request)).Returns(newLocation);
        _mockMapper.Setup(m => m.Map<LocationDto>(newLocation)).Returns(locationDto);
        _mockBackgroundJobClient.Setup(c => c.Enqueue(It.IsAny<Expression<Func<Task>>>(), It.IsAny<TimeSpan?>()))
            .Returns("daily-job-id");

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        _mockContext.Verify(c => c.LocationJobs.AddAsync(It.IsAny<LocationJob>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region UpdateLocationJobStatus Tests

    [Test]
    public void UpdateLocationJobStatus_WhenJobExists_UpdatesStatusToCompleted()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var jobId = "test-job-id";

        var locationJob = new LocationJob
        {
            Id = Guid.NewGuid(),
            LocationId = locationId,
            JobId = jobId,
            Status = "Pending"
        };

        var locationJobs = new List<LocationJob> { locationJob }.AsQueryable();

        var mockSet = new Mock<DbSet<LocationJob>>();
        mockSet.As<IQueryable<LocationJob>>().Setup(m => m.Provider).Returns(locationJobs.Provider);
        mockSet.As<IQueryable<LocationJob>>().Setup(m => m.Expression).Returns(locationJobs.Expression);
        mockSet.As<IQueryable<LocationJob>>().Setup(m => m.ElementType).Returns(locationJobs.ElementType);
        mockSet.As<IQueryable<LocationJob>>().Setup(m => m.GetEnumerator()).Returns(locationJobs.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefault(It.IsAny<Func<LocationJob, bool>>()))
            .Returns(locationJob);

        _mockContext.Setup(c => c.LocationJobs).Returns(mockSet.Object);

        // Act
        _service.UpdateLocationJobStatus(locationId, jobId);

        // Assert
        locationJob.Status.Should().Be("Completed");
        _mockContext.Verify(c => c.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void UpdateLocationJobStatus_WhenJobDoesNotExist_DoesNothing()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var jobId = "non-existent-job";

        var locationJobs = new List<LocationJob>().AsQueryable();

        var mockSet = new Mock<DbSet<LocationJob>>();
        mockSet.As<IQueryable<LocationJob>>().Setup(m => m.Provider).Returns(locationJobs.Provider);
        mockSet.As<IQueryable<LocationJob>>().Setup(m => m.Expression).Returns(locationJobs.Expression);
        mockSet.As<IQueryable<LocationJob>>().Setup(m => m.ElementType).Returns(locationJobs.ElementType);
        mockSet.As<IQueryable<LocationJob>>().Setup(m => m.GetEnumerator()).Returns(locationJobs.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefault(It.IsAny<Func<LocationJob, bool>>()))
            .Returns((LocationJob?)null);

        _mockContext.Setup(c => c.LocationJobs).Returns(mockSet.Object);

        // Act
        _service.UpdateLocationJobStatus(locationId, jobId);

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region WaitForLocationWeatherDataAsync Tests

    [Test]
    public async Task WaitForLocationWeatherDataAsync_WhenNoPendingJob_ReturnsImmediately()
    {
        // Arrange
        var locationId = Guid.NewGuid();

        var locationJobs = new List<LocationJob>().AsQueryable();

        var mockSet = new Mock<DbSet<LocationJob>>();
        mockSet.As<IQueryable<LocationJob>>().Setup(m => m.Provider).Returns(locationJobs.Provider);
        mockSet.As<IQueryable<LocationJob>>().Setup(m => m.Expression).Returns(locationJobs.Expression);
        mockSet.As<IQueryable<LocationJob>>().Setup(m => m.ElementType).Returns(locationJobs.ElementType);
        mockSet.As<IQueryable<LocationJob>>().Setup(m => m.GetEnumerator()).Returns(locationJobs.GetEnumerator());
        mockSet.Setup(m => m.OrderByDescending(It.IsAny<Expression<Func<LocationJob, DateTime>>>()).FirstOrDefaultAsync(It.IsAny<Func<LocationJob, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LocationJob?)null);

        _mockContext.Setup(c => c.LocationJobs).Returns(mockSet.Object);

        // Act
        await _service.WaitForLocationWeatherDataAsync(locationId);

        // Assert
        _mockContext.Verify(c => c.LocationJobs, Times.Once);
    }

    [Test]
    public async Task WaitForLocationWeatherDataAsync_WhenJobDoesNotExistInHangfire_EnqueuesNewJob()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var jobId = "pending-job-id";

        var pendingJob = new LocationJob
        {
            Id = Guid.NewGuid(),
            LocationId = locationId,
            JobId = jobId,
            Status = "Pending",
            JobCreatedAt = DateTime.UtcNow
        };

        var location = new Location { Id = locationId, Name = "Test" };

        var locationJobs = new List<LocationJob> { pendingJob }.AsQueryable();
        var locations = new List<Location> { location }.AsQueryable();

        var mockJobSet = new Mock<DbSet<LocationJob>>();
        mockJobSet.As<IQueryable<LocationJob>>().Setup(m => m.Provider).Returns(locationJobs.Provider);
        mockJobSet.As<IQueryable<LocationJob>>().Setup(m => m.Expression).Returns(locationJobs.Expression);
        mockJobSet.As<IQueryable<LocationJob>>().Setup(m => m.ElementType).Returns(locationJobs.ElementType);
        mockJobSet.As<IQueryable<LocationJob>>().Setup(m => m.GetEnumerator()).Returns(locationJobs.GetEnumerator());

        var mockLocationSet = new Mock<DbSet<Location>>();
        mockLocationSet.As<IQueryable<Location>>().Setup(m => m.Provider).Returns(locations.Provider);
        mockLocationSet.As<IQueryable<Location>>().Setup(m => m.Expression).Returns(locations.Expression);
        mockLocationSet.As<IQueryable<Location>>().Setup(m => m.ElementType).Returns(locations.ElementType);
        mockLocationSet.As<IQueryable<Location>>().Setup(m => m.GetEnumerator()).Returns(locations.GetEnumerator());
        mockLocationSet.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        _mockContext.Setup(c => c.LocationJobs).Returns(mockJobSet.Object);
        _mockContext.Setup(c => c.Locations).Returns(mockLocationSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Mock Hangfire monitoring API
        var mockMonitoringApi = new Mock<Hangfire.Storage.IMonitoringApi>();
        mockMonitoringApi.Setup(m => m.JobDetails(jobId)).Returns((Hangfire.Storage.JobDetails?)null);

        var mockJobStorage = new Mock<JobStorage>();
        mockJobStorage.Setup(m => m.GetMonitoringApi()).Returns(mockMonitoringApi.Object);

        JobStorage.Current = mockJobStorage.Object;

        // Act
        await _service.WaitForLocationWeatherDataAsync(locationId);

        // Assert
        _mockBackgroundJobClient.Verify(c => c.Enqueue(It.IsAny<Expression<Func<Task>>>(), It.IsAny<TimeSpan?>()), Times.Once);
    }

    #endregion
}

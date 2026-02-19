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
using AutoMapper;
using NUnit.Framework;

namespace weatherapp.tests.Services;

[TestFixture]
public class TrackLocationServiceTests
{
    private Mock<AppDbContext> _mockContext = null!;
    private Mock<IMapper> _mockMapper = null!;
    private TrackLocationService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<AppDbContext>();
        _mockMapper = new Mock<IMapper>();
        _service = new TrackLocationService(_mockContext.Object, _mockMapper.Object);
    }

    #region GetAllByUserIdAsync Tests

    [Test]
    public async Task GetAllByUserIdAsync_WhenTrackedLocationsExist_ReturnsAllTrackedLocations()
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
                isFavorite = true,
                DisplayName = "Home",
                Location = new Location
                {
                    Id = locationId,
                    Name = "London",
                    Latitude = 51.5074,
                    Longitude = -0.1278,
                    Country = "UK"
                }
            }
        }.AsQueryable();

        var trackLocationDtos = new List<TrackLocationDto>
        {
            new()
            {
                Id = trackLocations.First().Id,
                UserId = userId,
                LocationId = locationId,
                IsFavorite = true,
                DisplayName = "Home"
            }
        };

        var mockSet = new Mock<DbSet<TrackLocation>>();
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Provider).Returns(trackLocations.Provider);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Expression).Returns(trackLocations.Expression);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.ElementType).Returns(trackLocations.ElementType);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.GetEnumerator()).Returns(trackLocations.GetEnumerator());

        _mockContext.Setup(c => c.TrackLocations).Returns(mockSet.Object);
        _mockMapper.Setup(m => m.Map<List<TrackLocationDto>>(It.IsAny<List<TrackLocation>>()))
            .Returns(trackLocationDtos);

        // Act
        var result = await _service.GetAllByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(1);
        result.First().LocationId.Should().Be(locationId);
        result.First().IsFavorite.Should().BeTrue();
    }

    [Test]
    public async Task GetAllByUserIdAsync_WhenNoTrackedLocations_ReturnsEmptyList()
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
        _mockMapper.Setup(m => m.Map<List<TrackLocationDto>>(It.IsAny<List<TrackLocation>>()))
            .Returns(new List<TrackLocationDto>());

        // Act
        var result = await _service.GetAllByUserIdAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateAsync Tests

    [Test]
    public async Task CreateAsync_WithValidRequest_CreatesTrackLocation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var trackLocationId = Guid.NewGuid();

        var request = new CreateTrackLocationRequest
        {
            LocationId = locationId,
            DisplayName = "Work",
            IsFavorite = false
        };

        var newTrackLocation = new TrackLocation
        {
            Id = trackLocationId,
            UserId = userId,
            LocationId = locationId,
            isFavorite = false,
            DisplayName = "Work"
        };

        var trackLocationDto = new TrackLocationDto
        {
            Id = trackLocationId,
            UserId = userId,
            LocationId = locationId,
            IsFavorite = false,
            DisplayName = "Work"
        };

        var locations = new List<Location> { new Location { Id = locationId } }.AsQueryable();
        var trackLocations = new List<TrackLocation>().AsQueryable();

        var mockLocationSet = new Mock<DbSet<Location>>();
        mockLocationSet.As<IQueryable<Location>>().Setup(m => m.Provider).Returns(locations.Provider);
        mockLocationSet.As<IQueryable<Location>>().Setup(m => m.Expression).Returns(locations.Expression);
        mockLocationSet.As<IQueryable<Location>>().Setup(m => m.ElementType).Returns(locations.ElementType);
        mockLocationSet.As<IQueryable<Location>>().Setup(m => m.GetEnumerator()).Returns(locations.GetEnumerator());
        mockLocationSet.Setup(m => m.AnyAsync(It.IsAny<Func<Location, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var mockTrackSet = new Mock<DbSet<TrackLocation>>();
        mockTrackSet.As<IQueryable<TrackLocation>>().Setup(m => m.Provider).Returns(trackLocations.Provider);
        mockTrackSet.As<IQueryable<TrackLocation>>().Setup(m => m.Expression).Returns(trackLocations.Expression);
        mockTrackSet.As<IQueryable<TrackLocation>>().Setup(m => m.ElementType).Returns(trackLocations.ElementType);
        mockTrackSet.As<IQueryable<TrackLocation>>().Setup(m => m.GetEnumerator()).Returns(trackLocations.GetEnumerator());
        mockTrackSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<TrackLocation, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TrackLocation?)null);

        _mockContext.Setup(c => c.Locations).Returns(mockLocationSet.Object);
        _mockContext.Setup(c => c.TrackLocations).Returns(mockTrackSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<TrackLocation>(request)).Returns(newTrackLocation);
        _mockMapper.Setup(m => m.Map<TrackLocationDto>(It.IsAny<TrackLocation>())).Returns(trackLocationDto);

        // Act
        var result = await _service.CreateAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.LocationId.Should().Be(locationId);
        _mockContext.Verify(c => c.TrackLocations.AddAsync(It.IsAny<TrackLocation>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreateAsync_WhenLocationDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var request = new CreateTrackLocationRequest
        {
            LocationId = locationId
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

        // Act & Assert
        await _service.Invoking(s => s.CreateAsync(userId, request))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Location with ID {locationId} does not exist.");
    }

    [Test]
    public async Task CreateAsync_WhenAlreadyTrackingLocation_ThrowsDuplicateTrackLocationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var request = new CreateTrackLocationRequest
        {
            LocationId = locationId
        };

        var existingTrackLocation = new TrackLocation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LocationId = locationId
        };

        var locations = new List<Location> { new Location { Id = locationId } }.AsQueryable();
        var trackLocations = new List<TrackLocation> { existingTrackLocation }.AsQueryable();

        var mockLocationSet = new Mock<DbSet<Location>>();
        mockLocationSet.As<IQueryable<Location>>().Setup(m => m.Provider).Returns(locations.Provider);
        mockLocationSet.As<IQueryable<Location>>().Setup(m => m.Expression).Returns(locations.Expression);
        mockLocationSet.As<IQueryable<Location>>().Setup(m => m.ElementType).Returns(locations.ElementType);
        mockLocationSet.As<IQueryable<Location>>().Setup(m => m.GetEnumerator()).Returns(locations.GetEnumerator());
        mockLocationSet.Setup(m => m.AnyAsync(It.IsAny<Func<Location, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var mockTrackSet = new Mock<DbSet<TrackLocation>>();
        mockTrackSet.As<IQueryable<TrackLocation>>().Setup(m => m.Provider).Returns(trackLocations.Provider);
        mockTrackSet.As<IQueryable<TrackLocation>>().Setup(m => m.Expression).Returns(trackLocations.Expression);
        mockTrackSet.As<IQueryable<TrackLocation>>().Setup(m => m.ElementType).Returns(trackLocations.ElementType);
        mockTrackSet.As<IQueryable<TrackLocation>>().Setup(m => m.GetEnumerator()).Returns(trackLocations.GetEnumerator());
        mockTrackSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<TrackLocation, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTrackLocation);

        _mockContext.Setup(c => c.Locations).Returns(mockLocationSet.Object);
        _mockContext.Setup(c => c.TrackLocations).Returns(mockTrackSet.Object);

        // Act & Assert
        await _service.Invoking(s => s.CreateAsync(userId, request))
            .Should().ThrowAsync<DuplicateTrackLocationException>()
            .WithMessage($"User is already tracking location with ID {locationId}.");
    }

    #endregion

    #region UpdateAsync Tests

    [Test]
    public async Task UpdateAsync_WithValidRequest_UpdatesTrackLocation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var trackLocationId = Guid.NewGuid();

        var request = new UpdateTrackLocationRequest
        {
            DisplayName = "Updated Home",
            IsFavorite = true
        };

        var existingTrackLocation = new TrackLocation
        {
            Id = trackLocationId,
            UserId = userId,
            LocationId = Guid.NewGuid(),
            isFavorite = false,
            DisplayName = "Old Name"
        };

        var updatedTrackLocationDto = new TrackLocationDto
        {
            Id = trackLocationId,
            UserId = userId,
            IsFavorite = true,
            DisplayName = "Updated Home"
        };

        var trackLocations = new List<TrackLocation> { existingTrackLocation }.AsQueryable();

        var mockSet = new Mock<DbSet<TrackLocation>>();
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Provider).Returns(trackLocations.Provider);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Expression).Returns(trackLocations.Expression);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.ElementType).Returns(trackLocations.ElementType);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.GetEnumerator()).Returns(trackLocations.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<TrackLocation, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTrackLocation);

        _mockContext.Setup(c => c.TrackLocations).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<TrackLocationDto>(It.IsAny<TrackLocation>())).Returns(updatedTrackLocationDto);

        // Act
        var result = await _service.UpdateAsync(userId, trackLocationId, request);

        // Assert
        result.Should().NotBeNull();
        result.DisplayName.Should().Be("Updated Home");
        result.IsFavorite.Should().BeTrue();
        existingTrackLocation.isFavorite.Should().BeTrue();
        existingTrackLocation.DisplayName.Should().Be("Updated Home");
    }

    [Test]
    public async Task UpdateAsync_WhenTrackLocationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var trackLocationId = Guid.NewGuid();

        var request = new UpdateTrackLocationRequest
        {
            DisplayName = "Test"
        };

        var trackLocations = new List<TrackLocation>().AsQueryable();

        var mockSet = new Mock<DbSet<TrackLocation>>();
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Provider).Returns(trackLocations.Provider);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Expression).Returns(trackLocations.Expression);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.ElementType).Returns(trackLocations.ElementType);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.GetEnumerator()).Returns(trackLocations.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<TrackLocation, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TrackLocation?)null);

        _mockContext.Setup(c => c.TrackLocations).Returns(mockSet.Object);

        // Act & Assert
        await _service.Invoking(s => s.UpdateAsync(userId, trackLocationId, request))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Tracked location with ID {trackLocationId} not found for user ID {userId}.");
    }

    [Test]
    public async Task UpdateAsync_WithOnlyDisplayName_UpdatesOnlyDisplayName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var trackLocationId = Guid.NewGuid();

        var request = new UpdateTrackLocationRequest
        {
            DisplayName = "New Name"
        };

        var existingTrackLocation = new TrackLocation
        {
            Id = trackLocationId,
            UserId = userId,
            LocationId = Guid.NewGuid(),
            isFavorite = false,
            DisplayName = "Old Name"
        };

        var updatedTrackLocationDto = new TrackLocationDto
        {
            Id = trackLocationId,
            UserId = userId,
            IsFavorite = false,
            DisplayName = "New Name"
        };

        var trackLocations = new List<TrackLocation> { existingTrackLocation }.AsQueryable();

        var mockSet = new Mock<DbSet<TrackLocation>>();
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Provider).Returns(trackLocations.Provider);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Expression).Returns(trackLocations.Expression);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.ElementType).Returns(trackLocations.ElementType);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.GetEnumerator()).Returns(trackLocations.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<TrackLocation, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTrackLocation);

        _mockContext.Setup(c => c.TrackLocations).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<TrackLocationDto>(It.IsAny<TrackLocation>())).Returns(updatedTrackLocationDto);

        // Act
        var result = await _service.UpdateAsync(userId, trackLocationId, request);

        // Assert
        existingTrackLocation.DisplayName.Should().Be("New Name");
        existingTrackLocation.isFavorite.Should().BeFalse(); // Unchanged
    }

    [Test]
    public async Task UpdateAsync_WithOnlyIsFavorite_UpdatesOnlyIsFavorite()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var trackLocationId = Guid.NewGuid();

        var request = new UpdateTrackLocationRequest
        {
            IsFavorite = true
        };

        var existingTrackLocation = new TrackLocation
        {
            Id = trackLocationId,
            UserId = userId,
            LocationId = Guid.NewGuid(),
            isFavorite = false,
            DisplayName = "Home"
        };

        var updatedTrackLocationDto = new TrackLocationDto
        {
            Id = trackLocationId,
            UserId = userId,
            IsFavorite = true,
            DisplayName = "Home"
        };

        var trackLocations = new List<TrackLocation> { existingTrackLocation }.AsQueryable();

        var mockSet = new Mock<DbSet<TrackLocation>>();
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Provider).Returns(trackLocations.Provider);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Expression).Returns(trackLocations.Expression);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.ElementType).Returns(trackLocations.ElementType);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.GetEnumerator()).Returns(trackLocations.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<TrackLocation, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTrackLocation);

        _mockContext.Setup(c => c.TrackLocations).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<TrackLocationDto>(It.IsAny<TrackLocation>())).Returns(updatedTrackLocationDto);

        // Act
        var result = await _service.UpdateAsync(userId, trackLocationId, request);

        // Assert
        existingTrackLocation.isFavorite.Should().BeTrue();
        existingTrackLocation.DisplayName.Should().Be("Home"); // Unchanged
    }

    #endregion

    #region DeleteAsync Tests

    [Test]
    public async Task DeleteAsync_WithValidData_RemovesTrackLocation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var trackLocationId = Guid.NewGuid();

        var existingTrackLocation = new TrackLocation
        {
            Id = trackLocationId,
            UserId = userId,
            LocationId = Guid.NewGuid()
        };

        var trackLocations = new List<TrackLocation> { existingTrackLocation }.AsQueryable();

        var mockSet = new Mock<DbSet<TrackLocation>>();
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Provider).Returns(trackLocations.Provider);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Expression).Returns(trackLocations.Expression);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.ElementType).Returns(trackLocations.ElementType);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.GetEnumerator()).Returns(trackLocations.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<TrackLocation, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTrackLocation);

        _mockContext.Setup(c => c.TrackLocations).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _service.DeleteAsync(userId, trackLocationId);

        // Assert
        _mockContext.Verify(c => c.TrackLocations.Remove(existingTrackLocation), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_WhenTrackLocationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var trackLocationId = Guid.NewGuid();

        var trackLocations = new List<TrackLocation>().AsQueryable();

        var mockSet = new Mock<DbSet<TrackLocation>>();
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Provider).Returns(trackLocations.Provider);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.Expression).Returns(trackLocations.Expression);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.ElementType).Returns(trackLocations.ElementType);
        mockSet.As<IQueryable<TrackLocation>>().Setup(m => m.GetEnumerator()).Returns(trackLocations.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<TrackLocation, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TrackLocation?)null);

        _mockContext.Setup(c => c.TrackLocations).Returns(mockSet.Object);

        // Act & Assert
        await _service.Invoking(s => s.DeleteAsync(userId, trackLocationId))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Failed to delete Tracked Location with ID {trackLocationId}");
    }

    #endregion
}

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
public class AuthServiceTests
{
    private Mock<AppDbContext> _mockContext = null!;
    private Mock<IMapper> _mockMapper = null!;
    private AuthService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<AppDbContext>();
        _mockMapper = new Mock<IMapper>();
        _service = new AuthService(_mockContext.Object, _mockMapper.Object);
    }

    #region Register Tests

    [Test]
    public async Task Register_WithValidRequest_CreatesNewUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new RegisterRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "SecurePass123!"
        };

        var newUser = new User
        {
            Id = userId,
            Name = request.Name,
            Email = request.Email,
            PasswordHash = "hashedPassword"
        };

        var userDto = new UserDto
        {
            Id = userId,
            Name = request.Name,
            Email = request.Email
        };

        var users = new List<User>().AsQueryable();

        var mockSet = new Mock<DbSet<User>>();
        mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<User, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockContext.Setup(c => c.Users).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<UserDto>(It.IsAny<User>())).Returns(userDto);

        // Act
        var result = await _service.Register(request);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(request.Email);
        result.Name.Should().Be(request.Name);
        _mockContext.Verify(c => c.Users.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Register_WithExistingEmail_ThrowsDuplicateEmailException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "John Doe",
            Email = "existing@example.com",
            Password = "SecurePass123!"
        };

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email
        };

        var users = new List<User> { existingUser }.AsQueryable();

        var mockSet = new Mock<DbSet<User>>();
        mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<User, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockContext.Setup(c => c.Users).Returns(mockSet.Object);

        // Act & Assert
        await _service.Invoking(s => s.Register(request))
            .Should().ThrowAsync<DuplicateEmailException>()
            .WithMessage("Email address is already registered.");
    }

    [Test]
    public async Task Register_HashesPasswordBeforeStoring()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new RegisterRequest
        {
            Name = "Jane Doe",
            Email = "jane@example.com",
            Password = "SecurePass123!"
        };

        var users = new List<User>().AsQueryable();

        var mockSet = new Mock<DbSet<User>>();
        mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<User, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockContext.Setup(c => c.Users).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        User? capturedUser = null;
        _mockContext.Setup(c => c.Users.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .ReturnsAsync(It.IsAny<User>());

        // Act
        await _service.Register(request);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().NotBeNullOrEmpty();
        capturedUser.PasswordHash.Should().NotBe(request.Password);
        BCrypt.Net.BCrypt.Verify(request.Password, capturedUser.PasswordHash).Should().BeTrue();
    }

    #endregion

    #region Login Tests

    [Test]
    public async Task Login_WithValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var password = "SecurePass123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = password
        };

        var existingUser = new User
        {
            Id = userId,
            Name = "Test User",
            Email = request.Email,
            PasswordHash = passwordHash
        };

        var users = new List<User> { existingUser }.AsQueryable();

        var mockSet = new Mock<DbSet<User>>();
        mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<User, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockContext.Setup(c => c.Users).Returns(mockSet.Object);

        // Act
        var result = await _service.Login(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Name.Should().Be("Test User");
        result.Email.Should().Be(request.Email);
    }

    [Test]
    public async Task Login_WithNonExistentEmail_ThrowsForbiddenException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "SomePassword123!"
        };

        var users = new List<User>().AsQueryable();

        var mockSet = new Mock<DbSet<User>>();
        mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<User, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockContext.Setup(c => c.Users).Returns(mockSet.Object);

        // Act & Assert
        await _service.Invoking(s => s.Login(request))
            .Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Invalid email or password.");
    }

    [Test]
    public async Task Login_WithInvalidPassword_ThrowsForbiddenException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var correctPassword = "SecurePass123!";
        var wrongPassword = "WrongPassword123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword);

        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = wrongPassword
        };

        var existingUser = new User
        {
            Id = userId,
            Email = request.Email,
            PasswordHash = passwordHash
        };

        var users = new List<User> { existingUser }.AsQueryable();

        var mockSet = new Mock<DbSet<User>>();
        mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<User, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockContext.Setup(c => c.Users).Returns(mockSet.Object);

        // Act & Assert
        await _service.Invoking(s => s.Login(request))
            .Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Invalid email or password.");
    }

    #endregion

    #region Update Tests

    [Test]
    public async Task Update_WithValidRequest_UpdatesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateRequest
        {
            Name = "Updated Name",
            Email = "updated@example.com"
        };

        var existingUser = new User
        {
            Id = userId,
            Name = "Original Name",
            Email = "original@example.com",
            PasswordHash = "hash"
        };

        var updatedUserDto = new UserDto
        {
            Id = userId,
            Name = request.Name,
            Email = request.Email
        };

        var users = new List<User> { existingUser }.AsQueryable();

        var mockSet = new Mock<DbSet<User>>();
        mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<User, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockContext.Setup(c => c.Users).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<UserDto>(existingUser)).Returns(updatedUserDto);

        // Act
        var result = await _service.Update(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Email.Should().Be(request.Email);
        existingUser.Name.Should().Be(request.Name);
        existingUser.Email.Should().Be(request.Email);
    }

    [Test]
    public async Task Update_WhenUserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateRequest
        {
            Name = "Updated Name",
            Email = "updated@example.com"
        };

        var users = new List<User>().AsQueryable();

        var mockSet = new Mock<DbSet<User>>();
        mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<User, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockContext.Setup(c => c.Users).Returns(mockSet.Object);

        // Act & Assert
        await _service.Invoking(s => s.Update(userId, request))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("User not found.");
    }

    [Test]
    public async Task Update_WhenNewEmailTakenByAnotherUser_ThrowsDuplicateEmailException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateRequest
        {
            Name = "Updated Name",
            Email = "taken@example.com"
        };

        var existingUser = new User
        {
            Id = userId,
            Email = "original@example.com",
            PasswordHash = "hash"
        };

        var userWithSameEmail = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email
        };

        var users = new List<User> { existingUser, userWithSameEmail }.AsQueryable();

        var mockSet = new Mock<DbSet<User>>();
        mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

        var callCount = 0;
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<User, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync<Func<User, bool>, CancellationToken>((predicate, _) =>
            {
                callCount++;
                return callCount == 1 ? existingUser : userWithSameEmail;
            });

        _mockContext.Setup(c => c.Users).Returns(mockSet.Object);

        // Act & Assert
        await _service.Invoking(s => s.Update(userId, request))
            .Should().ThrowAsync<DuplicateEmailException>()
            .WithMessage("Email address is already registered by another user.");
    }

    [Test]
    public async Task Update_WhenEmailUnchanged_AllowsUpdate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateRequest
        {
            Name = "Updated Name",
            Email = "same@example.com"
        };

        var existingUser = new User
        {
            Id = userId,
            Name = "Original Name",
            Email = "same@example.com",
            PasswordHash = "hash"
        };

        var updatedUserDto = new UserDto
        {
            Id = userId,
            Name = request.Name,
            Email = request.Email
        };

        var users = new List<User> { existingUser }.AsQueryable();

        var mockSet = new Mock<DbSet<User>>();
        mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());
        mockSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<User, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockContext.Setup(c => c.Users).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<UserDto>(existingUser)).Returns(updatedUserDto);

        // Act
        var result = await _service.Update(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
    }

    #endregion
}

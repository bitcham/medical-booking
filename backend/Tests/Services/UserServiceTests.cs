using Core.Application.Dtos.Requests;
using Core.Application.Exceptions;
using Core.Application.Repositories.Contracts;
using Core.Application.Services;
using Core.Application.Services.Impl;
using Core.Domain.Entities;
using Moq;
using Xunit;

namespace backend.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userService = new UserService(_userRepositoryMock.Object, new SimplePasswordHasher());
    }

    [Fact]
    public async Task Register_ShouldReturnUserResponse_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new RegisterUserRequest("test@example.com", "password123", "testuser");
        
        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        _userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => 
            {
                // Simulate database generating an ID
                user.Id = Guid.NewGuid(); 
                return user;
            });

        // Act
        var result = await _userService.Register(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.Username, result.Username);
        Assert.NotEqual(Guid.Empty, result.Id);

        _userRepositoryMock.Verify(repo => repo.AddAsync(It.Is<User>(u => 
            u.Email == request.Email && 
            u.Username == request.Username &&
            u.PasswordHash != request.Password // Ensure password is hashed
        )), Times.Once);
    }

    [Fact]
    public async Task Register_ShouldThrowDuplicateEmailException_WhenUserAlreadyExists()
    {
        // Arrange
        var request = new RegisterUserRequest("existing@example.com", "password123", "existinguser");
        var existingUser = User.Register(request.Email, "hashed_password", request.Username);

        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateEmailException>(() => _userService.Register(request));

        _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
    }
}

public class SimplePasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return password + "_hashed";
    }

    public bool Verify(string password, string hashedPassword)
    {
        return Hash(password) == hashedPassword;
    }
}
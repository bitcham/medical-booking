using Core.Application.Dtos.Requests;
using Core.Application.Exceptions;
using Core.Application.Repositories.Contracts;
using Core.Application.Services;
using Core.Application.Services.Impl;
using Core.Domain.Entities;
using FluentAssertions;
using Moq;
using backend.Tests.Fixtures;

namespace backend.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new UserService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region Register Tests

    [Fact]
    public async Task Register_WhenEmailExists_ShouldThrowDuplicateEmailException()
    {
        // Arrange
        var request = new RegisterUserRequest("test@test.com", "password", "John", "Doe");
        var existingUser = TestFixtures.Users.Create(email: "test@test.com");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var act = () => _sut.Register(request);

        // Assert
        await act.Should().ThrowAsync<DuplicateEmailException>();
    }

    [Fact]
    public async Task Register_WhenValid_ShouldCreateUserAndReturnResponse()
    {
        // Arrange
        var request = new RegisterUserRequest("test@test.com", "password", "John", "Doe");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(x => x.Hash(request.Password))
            .Returns("hashed_password");

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);

        // Act
        var result = await _sut.Register(request);

        // Assert
        result.Email.Should().Be(request.Email);
        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);
        
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WhenUserNotFound_ShouldThrowUserNotFoundException()
    {
        // Arrange
        var request = new LoginRequest("test@test.com", "password");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.Login(request);

        // Assert
        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task Login_WhenPasswordInvalid_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest("test@test.com", "wrong_password");
        var user = TestFixtures.Users.Create(email: "test@test.com", passwordHash: "hashed_password");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(request.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var act = () => _sut.Login(request);

        // Assert
        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task Login_WhenValid_ShouldReturnUserResponse()
    {
        // Arrange
        var request = new LoginRequest("test@test.com", "password");
        var user = TestFixtures.Users.Create(email: "test@test.com", passwordHash: "hashed_password");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(request.Password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _sut.Login(request);

        // Assert
        result.Email.Should().Be(user.Email);
        result.FirstName.Should().Be(user.FirstName);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenUserNotFound_ShouldThrowUserNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.GetByIdAsync(userId);

        // Assert
        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUserResponse()
    {
        // Arrange
        var user = TestFixtures.Users.Create();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Email.Should().Be(user.Email);
    }

    #endregion
}

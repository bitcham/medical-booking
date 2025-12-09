using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;
using Core.Application.Exceptions;
using Core.Application.Options;
using Core.Application.Repositories.Contracts;
using Core.Application.Services;
using Core.Application.Services.Impl;
using Core.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using backend.Tests.Fixtures;

namespace backend.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IPatientService> _patientServiceMock;
    private readonly Mock<IClinicianService> _clinicianServiceMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _patientServiceMock = new Mock<IPatientService>();
        _clinicianServiceMock = new Mock<IClinicianService>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        var jwtOptions = Options.Create(new JwtOptions
        {
            Key = "test-secret-key-with-minimum-length-123",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpireHours = 1,
            RefreshTokenExpireHours = 24
        });

        _sut = new AuthService(
            _userServiceMock.Object,
            _patientServiceMock.Object,
            _clinicianServiceMock.Object,
            _jwtTokenGeneratorMock.Object,
            _refreshTokenRepositoryMock.Object,
            jwtOptions,
            _unitOfWorkMock.Object
        );
    }

    #region Register Tests

    [Fact]
    public async Task Register_ShouldRegisterUserAndReturnTokens()
    {
        // Arrange
        var request = new RegisterUserRequest("test@test.com", "password", "John", "Doe");
        var userResponse = new UserResponse(Guid.NewGuid(), "test@test.com", "John", "Doe", "FrontDesk");

        SetupTokenGeneration(userResponse);

        _userServiceMock
            .Setup(x => x.Register(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userResponse);

        // Act
        var result = await _sut.Register(request);

        // Assert
        result.User.Email.Should().Be(userResponse.Email);
        result.Token.Should().Be("jwt_token");
        result.RefreshToken.Should().Be("refresh_token");
        
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_ShouldAuthenticateAndReturnTokens()
    {
        // Arrange
        var request = new LoginRequest("test@test.com", "password");
        var userResponse = new UserResponse(Guid.NewGuid(), "test@test.com", "John", "Doe", "FrontDesk");

        SetupTokenGeneration(userResponse);

        _userServiceMock
            .Setup(x => x.Login(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userResponse);

        // Act
        var result = await _sut.Login(request);

        // Assert
        result.User.Email.Should().Be(userResponse.Email);
        result.Token.Should().Be("jwt_token");
    }

    #endregion

    #region RefreshToken Tests

    [Fact]
    public async Task RefreshToken_WhenTokenNotFound_ShouldThrowTokenNotFoundException()
    {
        // Arrange
        var token = "invalid_refresh_token";

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var act = () => _sut.RefreshToken(token, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TokenNotFoundException>();
    }

    [Fact]
    public async Task RefreshToken_WhenTokenExpired_ShouldThrowTokenNotValidException()
    {
        // Arrange
        var token = "expired_refresh_token";
        var expiredRefreshToken = TestFixtures.RefreshTokens.CreateExpired();
        expiredRefreshToken.Token = token;

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredRefreshToken);

        // Act
        var act = () => _sut.RefreshToken(token, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TokenNotValidException>();
    }

    [Fact]
    public async Task RefreshToken_WhenRevoked_ShouldThrowTokenNotValidException()
    {
        // Arrange
        var token = "revoked_refresh_token";
        var revokedRefreshToken = TestFixtures.RefreshTokens.CreateRevoked();
        revokedRefreshToken.Token = token;

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(revokedRefreshToken);

        // Act
        var act = () => _sut.RefreshToken(token, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TokenNotValidException>();
    }

    [Fact]
    public async Task RefreshToken_WhenValid_ShouldRevokeOldTokenAndReturnNewTokens()
    {
        // Arrange
        var token = "valid_refresh_token";
        var userId = Guid.NewGuid();
        var refreshToken = TestFixtures.RefreshTokens.Create(userId: userId, token: token);
        var userResponse = new UserResponse(userId, "test@test.com", "John", "Doe", "FrontDesk");

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        _userServiceMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userResponse);

        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(userId, userResponse.Email, userResponse.Role))
            .Returns("new_jwt_token");

        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateRefreshToken(userId, userResponse.Email))
            .Returns("new_refresh_token");

        // Act
        var result = await _sut.RefreshToken(token, CancellationToken.None);

        // Assert
        result.Token.Should().Be("new_jwt_token");
        result.RefreshToken.Should().Be("new_refresh_token");
        refreshToken.Revoked.Should().NotBeNull("old token should be revoked");
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_WhenTokenExists_ShouldRevokeToken()
    {
        // Arrange
        var token = "valid_refresh_token";
        var refreshToken = TestFixtures.RefreshTokens.Create(token: token);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        // Act
        await _sut.Logout(token, CancellationToken.None);

        // Assert
        refreshToken.Revoked.Should().NotBeNull();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Logout_WhenTokenNotFound_ShouldNotThrowAndNotSave()
    {
        // Arrange
        var token = "nonexistent_token";

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var act = () => _sut.Logout(token, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Logout_WhenTokenAlreadyRevoked_ShouldNotSaveAgain()
    {
        // Arrange
        var token = "revoked_token";
        var revokedToken = TestFixtures.RefreshTokens.CreateRevoked();
        revokedToken.Token = token;

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(revokedToken);

        // Act
        await _sut.Logout(token, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    private void SetupTokenGeneration(UserResponse userResponse)
    {
        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(userResponse.Id, userResponse.Email, userResponse.Role))
            .Returns("jwt_token");

        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateRefreshToken(userResponse.Id, userResponse.Email))
            .Returns("refresh_token");
    }
}

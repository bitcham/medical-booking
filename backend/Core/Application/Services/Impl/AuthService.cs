using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;
using Core.Application.Exceptions;
using Core.Application.Options;
using Core.Application.Repositories.Contracts;
using Core.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Core.Application.Services.Impl;

public class AuthService(
    IUserService userService, 
    IPatientService patientService,
    IClinicianService clinicianService,
    IJwtTokenGenerator jwtTokenGenerator, 
    IRefreshTokenRepository refreshTokenRepository, 
    IOptions<JwtOptions> jwtOptions,
    IUnitOfWork unitOfWork): IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthResponse> Register(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        var savedUser = await userService.Register(request, cancellationToken);
        var (token, refreshTokenString) = await GenerateTokensAsync(savedUser.Id, savedUser.Email, savedUser.Role, cancellationToken);
        return new AuthResponse(savedUser, token, refreshTokenString);
    }

    public async Task<AuthResponse> RegisterPatient(RegisterPatientRequest request, CancellationToken cancellationToken = default)
    {
        var savedPatient = await patientService.Register(request, cancellationToken);
        var (token, refreshTokenString) = await GenerateTokensAsync(savedPatient.User.Id, savedPatient.User.Email, savedPatient.User.Role, cancellationToken);
        return new AuthResponse(savedPatient.User, token, refreshTokenString);
    }

    public async Task<AuthResponse> RegisterClinician(RegisterClinicianRequest request, CancellationToken cancellationToken = default)
    {
        var savedClinician = await clinicianService.Register(request, cancellationToken);
        var (token, refreshTokenString) = await GenerateTokensAsync(savedClinician.User.Id, savedClinician.User.Email, savedClinician.User.Role, cancellationToken);
        return new AuthResponse(savedClinician.User, token, refreshTokenString);
    }

    public async Task<AuthResponse> Login(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var validUser = await userService.Login(request, cancellationToken);
        var (token, refreshTokenString) = await GenerateTokensAsync(validUser.Id, validUser.Email, validUser.Role, cancellationToken);
        return new AuthResponse(validUser, token, refreshTokenString);
    }

    public async Task<AuthResponse> RefreshToken(string refreshToken, CancellationToken cancellationToken)
    {
        var existingRefreshToken = await refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken)
            ?? throw new TokenNotFoundException("Invalid refresh token.");
        
        if(!existingRefreshToken.IsActive)
        {
            throw new TokenNotValidException("Refresh token is not valid.");
        }
        
        // Revoke the old refresh token
        existingRefreshToken.Revoked = DateTimeOffset.UtcNow;
        
        var user = await userService.GetByIdAsync(existingRefreshToken.UserId);
        var (token, refreshTokenString) = await GenerateTokensAsync(user.Id, user.Email, user.Role, cancellationToken);
        return new AuthResponse(user, token, refreshTokenString);
    }

    public async Task Logout(string refreshToken, CancellationToken cancellationToken)
    {
        var existingRefreshToken = await refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);
        
        if (existingRefreshToken is not null && existingRefreshToken.IsActive)
        {
            existingRefreshToken.Revoked = DateTimeOffset.UtcNow;
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<(string Token, string RefreshToken)> GenerateTokensAsync(
        Guid userId, string email, string role, CancellationToken cancellationToken)
    {
        var token = jwtTokenGenerator.GenerateToken(userId, email, role);
        var refreshTokenString = jwtTokenGenerator.GenerateRefreshToken(userId, email);

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenString,
            UserId = userId,
            Expires = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenExpireHours)
        };

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return (token, refreshTokenString);
    }
}

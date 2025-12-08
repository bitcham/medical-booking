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
    IJwtTokenGenerator jwtTokenGenerator, 
    IRefreshTokenRepository refreshTokenRepository, 
    IOptions<JwtOptions> jwtOptions,
    IUnitOfWork unitOfWork): IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthResponse> Register(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        var savedUser = await userService.Register(request, cancellationToken);
        
        var token = jwtTokenGenerator.GenerateToken(savedUser.Id, savedUser.Email);
        var refreshTokenString = jwtTokenGenerator.GenerateRefreshToken(savedUser.Id, savedUser.Email);

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenString,
            UserId = savedUser.Id,
            Expires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpireDays)
        };

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(savedUser, token, refreshTokenString);
    }

    public async Task<AuthResponse> RegisterPatient(RegisterPatientRequest request, CancellationToken cancellationToken = default)
    {
        var savedPatient = await patientService.Register(request, cancellationToken);
        
        var token = jwtTokenGenerator.GenerateToken(savedPatient.User.Id, savedPatient.User.Email);
        var refreshTokenString = jwtTokenGenerator.GenerateRefreshToken(savedPatient.User.Id, savedPatient.User.Email);

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenString,
            UserId = savedPatient.User.Id,
            Expires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpireDays)
        };

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(savedPatient.User, token, refreshTokenString);
    }

    public async Task<AuthResponse> Login(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var validUser = await userService.Login(request, cancellationToken);
        
        var token = jwtTokenGenerator.GenerateToken(validUser.Id, validUser.Email);
        var refreshTokenString = jwtTokenGenerator.GenerateRefreshToken(validUser.Id, validUser.Email);
        
        var refreshToken = new RefreshToken
        {
            Token = refreshTokenString,
            UserId = validUser.Id,
            Expires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpireDays)
        };
        
        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
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
        
        var user = await userService.GetByIdAsync(existingRefreshToken.UserId);
        
        var newJwtToken = jwtTokenGenerator.GenerateToken(user.Id, user.Email);
        var newRefreshTokenString = jwtTokenGenerator.GenerateRefreshToken(user.Id, user.Email);
        
        var newRefreshToken = new RefreshToken
        {
            Token = newRefreshTokenString,
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpireDays)
        };
        
        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new AuthResponse(user, newJwtToken, newRefreshTokenString);
    }
}

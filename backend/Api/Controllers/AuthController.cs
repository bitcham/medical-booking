using Asp.Versioning;
using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;
using Core.Application.Options;
using Core.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace backend.Controllers;

[ApiController]
public class AuthController(ILogger<AuthController> _logger, IAuthService authService, IOptions<JwtOptions> jwtOptions) : ControllerBase
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    [HttpPost(ApiEndpoints.Auth.Register)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.Register(request, cancellationToken);
        
        SetRefreshTokenCookie(result.RefreshToken);
        
        var response = result with { RefreshToken = string.Empty };
        
        return CreatedAtAction(
            actionName: nameof(UserController.GetUserById), 
            controllerName: "User", 
            routeValues: new { id = result.User.Id, version = "1.0" },
            value: response
        );
    }

    [HttpPost(ApiEndpoints.Auth.RegisterPatient)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<AuthResponse>> RegisterPatient(RegisterPatientRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterPatient(request, cancellationToken);
        
        SetRefreshTokenCookie(result.RefreshToken);
        
        var response = result with { RefreshToken = string.Empty };
        
        return CreatedAtAction(
            actionName: nameof(UserController.GetUserById), 
            controllerName: "User", 
            routeValues: new { id = result.User.Id, version = "1.0" },
            value: response
        );
    }

    [HttpPost(ApiEndpoints.Auth.Login)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.Login(request, cancellationToken);
        
        SetRefreshTokenCookie(result.RefreshToken);
        
        var response = result with { RefreshToken = string.Empty };
        
        return Ok(response);
    }

    [HttpPost(ApiEndpoints.Auth.RefreshToken)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> RefreshToken(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized("Refresh token is missing.");
        }

        var result = await authService.RefreshToken(refreshToken, cancellationToken);
        
        SetRefreshTokenCookie(result.RefreshToken);
        
        var response = result with { RefreshToken = string.Empty };
        
        return Ok(response);
    }
    
    
    
    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpireDays),
            SameSite = SameSiteMode.Strict,
            Secure = true // Ensure this is true in production (requires HTTPS)
        };
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}
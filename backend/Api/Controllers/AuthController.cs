using Asp.Versioning;
using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;
using Core.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
public class AuthController(ILogger<AuthController> _logger, IAuthService authService) : ControllerBase
{
    
    [HttpPost(ApiEndpoints.Auth.Register)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<UserResponse>> Register(RegisterUserRequest request)
    {
        var result = await authService.Register(request);
        
        return Created("", result);
    }

    [HttpPost(ApiEndpoints.Auth.Login)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserResponse>> Login(LoginRequest request)
    {
        var result = await authService.Login(request);
        
        return Ok(result);
    }
    
}
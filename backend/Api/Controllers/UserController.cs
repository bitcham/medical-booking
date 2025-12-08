using Core.Application.Dtos.Responses;
using Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
    
    [HttpGet(ApiEndpoints.Users.GetUserById)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserResponse>> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var foundUser = await userService.GetByIdAsync(id, cancellationToken);
        return Ok(foundUser);
    }
    
}
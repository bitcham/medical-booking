using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;

namespace Core.Application.Services;

public interface IAuthService
{
    Task<UserResponse> Register(RegisterUserRequest request);
    Task<UserResponse> Login(LoginRequest request);
}
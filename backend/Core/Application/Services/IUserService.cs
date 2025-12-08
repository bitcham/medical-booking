using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;

namespace Core.Application.Services;

public interface IUserService
{
    Task<UserResponse> Register(RegisterUserRequest request, CancellationToken cancellationToken = default);

    Task<UserResponse> Login(LoginRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
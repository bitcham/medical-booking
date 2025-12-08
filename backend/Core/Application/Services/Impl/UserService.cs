using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;
using Core.Application.Exceptions;
using Core.Application.Repositories.Contracts;
using Core.Domain.Entities;

namespace Core.Application.Services.Impl;

public class UserService(
    IUserRepository userRepository, 
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : IUserService
{
    public async Task<UserResponse> Register(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        if (await userRepository.GetByEmailAsync(request.Email, cancellationToken) is not null)
        {
            throw new DuplicateEmailException();
        }

        var passwordHash = passwordHasher.Hash(request.Password);
        
        var user = User.Register(request.Email, passwordHash, request.FirstName, request.LastName);

        var savedUser = await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UserResponse.FromEntity(savedUser);
        
    }

    public async Task<UserResponse> Login(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        
        if(user is null){
            throw new UserNotFoundException();
        }
        
        return passwordHasher.Verify(request.Password, user.PasswordHash) ?  UserResponse.FromEntity(user) : throw new InvalidCredentialsException("Username or password is incorrect.");
    }

    public async Task<UserResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await userRepository.GetByIdAsync(id, cancellationToken) is { } user
            ? UserResponse.FromEntity(user)
            : throw new UserNotFoundException();
    }
}

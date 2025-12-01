using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;
using Core.Application.Exceptions;
using Core.Application.Repositories.Contracts;
using Core.Domain.Entities;

namespace Core.Application.Services.Impl;

public class UserService(IUserRepository userRepository, IPasswordHasher passwordHasher) : IUserService
{
    public async Task<UserResponse> Register(RegisterUserRequest request)
    {
        if(await userRepository.GetByEmailAsync(request.Email) is not null)
        {
            throw new DuplicateEmailException();
        }

        var passwordHash = passwordHasher.Hash(request.Password);
        
        var user = User.Register(request.Email, passwordHash, request.Username);

        var savedUser = await userRepository.AddAsync(user);

        return UserResponse.FromEntity(savedUser);
        
    }

    public async Task<UserResponse> Login(LoginRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        
        if(user is null){
            throw new UserNotFoundException();
        }
        
        if(!user.VerifyPassword(request.Password, passwordHasher)){
            throw new InvalidCredentialsException("Username or password is incorrect.");
        }
        
        return UserResponse.FromEntity(user);
    }
}
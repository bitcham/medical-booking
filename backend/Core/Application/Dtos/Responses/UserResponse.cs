using Core.Domain.Entities;

namespace Core.Application.Dtos.Responses;

public record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName
    )
{
    public static UserResponse FromEntity(User user)
    {
        return new UserResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName
        );
    }
    
}
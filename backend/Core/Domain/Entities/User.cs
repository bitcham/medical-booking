using Core.Application.Services;
using Core.Domain.Enums;

namespace Core.Domain.Entities;

public class User : BaseEntity
{
    public Guid Id { get; set; }
    public required string Email { get; set; } = string.Empty;
    public required string PasswordHash { get; set; } = string.Empty;
    
    public required string FirstName { get; set; } = string.Empty;
    public required string LastName { get; set; } = string.Empty;
    
    public UserRole Role { get; set; } = UserRole.FrontDesk;

    protected User()
    {
    }
    
    public static User Register(string email, string passwordHash, string firstName, string lastName)
    {
         return new User
        {
            Email = email,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName
        };
    }
    
    public bool VerifyPassword(string passwordHash, IPasswordHasher passwordHasher)
    {
        return passwordHasher.Verify(passwordHash, PasswordHash);
    }
    
    public void UpdateName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
    
    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }
}
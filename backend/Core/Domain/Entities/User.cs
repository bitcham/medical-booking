using Core.Application.Services;

namespace Core.Domain.Entities;

public class User : BaseEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    
    public string Username { get; set; } = string.Empty;

    protected User()
    {
    }
    
    public static User Register(string email, string passwordHash, string username)
    {
         return new User
        {
            Email = email,
            PasswordHash = passwordHash,
            Username = username
        };
    }
    
    public bool VerifyPassword(string passwordHash, IPasswordHasher passwordHasher)
    {
        return passwordHasher.Verify(passwordHash, PasswordHash);
    }
    
    
}

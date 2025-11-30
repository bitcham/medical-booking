namespace backend.Models;

public class User : BaseEntity
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    
    public string Username { get; init; } = string.Empty;

    protected User()
    {
    }
    
    
}

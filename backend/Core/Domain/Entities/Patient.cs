using Core.Domain.ValueObjects;

namespace Core.Domain.Entities;

public class Patient : BaseEntity
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    
    public Address Address { get; set; } = null!;
    
}

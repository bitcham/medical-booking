namespace Core.Application.Dtos.Requests;

public record LoginRequest(
    string Email,
    string Password
)
{
    
}
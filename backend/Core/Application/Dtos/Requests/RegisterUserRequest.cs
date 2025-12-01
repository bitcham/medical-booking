namespace Core.Application.Dtos.Requests;

public record RegisterUserRequest(
    string Email,
    string Password,
    string Username);
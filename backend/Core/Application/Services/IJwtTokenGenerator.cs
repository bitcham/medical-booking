using Core.Application.Dtos.Responses;
using Core.Domain.Entities;

namespace Core.Application.Services;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, string email, string role);
    string GenerateRefreshToken(Guid userId, string email);
}
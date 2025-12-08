using Core.Domain.Entities;
using Core.Domain.ValueObjects;

namespace Core.Application.Dtos.Responses;

public record PatientResponse(
    Guid Id,
    UserResponse User,
    string PhoneNumber,
    DateTime DateOfBirth,
    Address Address
)
{
    public static PatientResponse FromEntity(Patient patient)
    {
        return new PatientResponse(
            patient.Id,
            UserResponse.FromEntity(patient.User),
            patient.PhoneNumber,
            patient.DateOfBirth,
            patient.Address
        );
    }
}

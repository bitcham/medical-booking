using Core.Domain.Entities;

namespace Core.Application.Dtos.Responses;

public record ClinicianResponse(
    Guid Id,
    UserResponse User,
    string LicenseNumber,
    string Specialization,
    string? Bio
)
{
    public static ClinicianResponse FromEntity(Clinician clinician)
    {
        return new ClinicianResponse(
            clinician.Id,
            UserResponse.FromEntity(clinician.User),
            clinician.LicenseNumber,
            clinician.Specialization,
            clinician.Bio
        );
    }
}

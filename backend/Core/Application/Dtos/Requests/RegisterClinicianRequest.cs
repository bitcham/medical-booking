namespace Core.Application.Dtos.Requests;

public record RegisterClinicianRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string LicenseNumber,
    string Specialization,
    string? Bio
);

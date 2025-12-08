namespace Core.Application.Dtos.Requests;

public record RegisterPatientRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateTime DateOfBirth,
    string Street,
    string City,
    string ZipCode,
    string Country
);
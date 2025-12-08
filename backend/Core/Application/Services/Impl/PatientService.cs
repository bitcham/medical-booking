using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;
using Core.Application.Exceptions;
using Core.Application.Repositories.Contracts;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.ValueObjects;

namespace Core.Application.Services.Impl;

public class PatientService(
    IUserRepository userRepository,
    IPatientRepository patientRepository, 
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork
    ) : IPatientService
{
    public async Task<PatientResponse> Register(RegisterPatientRequest request, CancellationToken cancellationToken = default)
    {
        if (await userRepository.GetByEmailAsync(request.Email, cancellationToken) is not null)
        {
            throw new DuplicateEmailException();
        }

        var passwordHash = passwordHasher.Hash(request.Password);

        // Create User
        var user = User.Register(request.Email, passwordHash, request.FirstName, request.LastName);
        user.Role = UserRole.Patient;

        await userRepository.AddAsync(user, cancellationToken);

        // Create Patient
        var address = new Address(request.Street, request.City, request.ZipCode, request.Country);
        var patient = new Patient
        {
            DateOfBirth = request.DateOfBirth,
            PhoneNumber = request.PhoneNumber,
            Address = address,
            User = user
        };

        await patientRepository.AddAsync(patient, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return PatientResponse.FromEntity(patient);
    }
}

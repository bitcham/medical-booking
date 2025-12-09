using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;
using Core.Application.Exceptions;
using Core.Application.Repositories.Contracts;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Application.Services.Impl;

public class ClinicianService(
    IUserRepository userRepository,
    IClinicianRepository clinicianRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork
) : IClinicianService
{
    public async Task<ClinicianResponse> Register(RegisterClinicianRequest request, CancellationToken cancellationToken = default)
    {
        if (await userRepository.GetByEmailAsync(request.Email, cancellationToken) is not null)
        {
            throw new DuplicateEmailException();
        }

        var passwordHash = passwordHasher.Hash(request.Password);

        // Create User
        var user = User.Register(request.Email, passwordHash, request.FirstName, request.LastName);
        user.Role = UserRole.Clinician;

        await userRepository.AddAsync(user, cancellationToken);

        // Create Clinician
        var clinician = new Clinician
        {
            LicenseNumber = request.LicenseNumber,
            Specialization = request.Specialization,
            Bio = request.Bio,
            User = user
        };

        await clinicianRepository.AddAsync(clinician, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ClinicianResponse.FromEntity(clinician);
    }

    public async Task<ClinicianResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var clinician = await clinicianRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ClinicianNotFoundException();
        return ClinicianResponse.FromEntity(clinician);
    }

    public async Task<ClinicianResponse> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var clinician = await clinicianRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new ClinicianNotFoundException();
        return ClinicianResponse.FromEntity(clinician);
    }

    public async Task<IEnumerable<ClinicianResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var clinicians = await clinicianRepository.GetAllAsync(cancellationToken);
        return clinicians.Select(ClinicianResponse.FromEntity);
    }
}

using Core.Domain.Entities;

namespace Core.Application.Repositories.Contracts;

public interface IPatientRepository
{
    Task<Patient> AddAsync(Patient patient, CancellationToken cancellationToken = default);
    Task<Patient?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

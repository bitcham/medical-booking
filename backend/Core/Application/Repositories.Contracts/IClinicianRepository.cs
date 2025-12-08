using Core.Domain.Entities;

namespace Core.Application.Repositories.Contracts;

public interface IClinicianRepository
{
    Task<Clinician> AddAsync(Clinician clinician, CancellationToken cancellationToken = default);
    Task<Clinician?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

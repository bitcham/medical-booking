using Core.Application.Repositories.Contracts;
using Core.Domain.Entities;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Impl;

public class ClinicianRepository(AppDbContext context) : IClinicianRepository
{
    public async Task<Clinician> AddAsync(Clinician clinician, CancellationToken cancellationToken = default)
    {
        await context.Clinicians.AddAsync(clinician, cancellationToken);
        return clinician;
    }

    public async Task<Clinician?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Clinicians
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Clinician?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Clinicians
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<Clinician>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Clinicians
            .Include(c => c.User)
            .ToListAsync(cancellationToken);
    }
}

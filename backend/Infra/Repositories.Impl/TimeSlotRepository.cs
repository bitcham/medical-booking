using Core.Application.Repositories.Contracts;
using Core.Domain.Entities;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Impl;

public class TimeSlotRepository(AppDbContext context) : ITimeSlotRepository
{
    public async Task<TimeSlot> AddAsync(TimeSlot timeSlot, CancellationToken cancellationToken = default)
    {
        await context.TimeSlots.AddAsync(timeSlot, cancellationToken);
        return timeSlot;
    }

    public async Task AddRangeAsync(IEnumerable<TimeSlot> timeSlots, CancellationToken cancellationToken = default)
    {
        await context.TimeSlots.AddRangeAsync(timeSlots, cancellationToken);
    }

    public async Task<TimeSlot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.TimeSlots
            .Include(t => t.Clinician)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<TimeSlot>> GetByClinicianIdAsync(Guid clinicianId, CancellationToken cancellationToken = default)
    {
        return await context.TimeSlots
            .Include(t => t.Clinician)
            .Where(t => t.ClinicianId == clinicianId)
            .OrderBy(t => t.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TimeSlot>> GetAvailableByClinicianIdAsync(Guid clinicianId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var startOfDay = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var endOfDay = new DateTimeOffset(date.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        return await context.TimeSlots
            .Include(t => t.Clinician)
            .Where(t => t.ClinicianId == clinicianId 
                && t.IsAvailable 
                && t.StartTime >= startOfDay 
                && t.StartTime <= endOfDay)
            .OrderBy(t => t.StartTime)
            .ToListAsync(cancellationToken);
    }
}

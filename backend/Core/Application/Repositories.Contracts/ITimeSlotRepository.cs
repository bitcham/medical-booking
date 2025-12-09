using Core.Domain.Entities;

namespace Core.Application.Repositories.Contracts;

public interface ITimeSlotRepository
{
    Task<TimeSlot> AddAsync(TimeSlot timeSlot, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TimeSlot> timeSlots, CancellationToken cancellationToken = default);
    Task<TimeSlot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TimeSlot>> GetByClinicianIdAsync(Guid clinicianId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TimeSlot>> GetAvailableByClinicianIdAsync(Guid clinicianId, DateOnly date, CancellationToken cancellationToken = default);
}

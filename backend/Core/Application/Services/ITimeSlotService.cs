using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;

namespace Core.Application.Services;

public interface ITimeSlotService
{
    Task<IEnumerable<TimeSlotResponse>> GenerateSlotsAsync(Guid clinicianId, GenerateTimeSlotsRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<TimeSlotResponse>> GetByClinicianIdAsync(Guid clinicianId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TimeSlotResponse>> GetAvailableByClinicianIdAsync(Guid clinicianId, DateOnly date, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

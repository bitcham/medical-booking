using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Application.Repositories.Contracts;

public interface IAppointmentRepository
{
    Task<Appointment> AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByClinicianIdAsync(Guid clinicianId, CancellationToken cancellationToken = default);
    Task<Appointment?> GetByTimeSlotIdAsync(Guid timeSlotId, CancellationToken cancellationToken = default);
}

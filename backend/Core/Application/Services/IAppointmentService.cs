using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;

namespace Core.Application.Services;

public interface IAppointmentService
{
    Task<AppointmentResponse> CreateAsync(Guid patientId, CreateAppointmentRequest request, CancellationToken cancellationToken = default);
    Task<AppointmentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AppointmentResponse>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AppointmentResponse>> GetByClinicianIdAsync(Guid clinicianId, CancellationToken cancellationToken = default);
    Task<AppointmentResponse> CancelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AppointmentResponse> ConfirmAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AppointmentResponse> RescheduleAsync(Guid id, RescheduleAppointmentRequest request, CancellationToken cancellationToken = default);
}

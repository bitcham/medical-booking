using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;

namespace Core.Application.Services;

public interface IPatientService
{
    Task<PatientResponse> Register(RegisterPatientRequest request, CancellationToken cancellationToken = default);
    Task<PatientResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PatientResponse> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

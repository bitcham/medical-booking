using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;

namespace Core.Application.Services;

public interface IPatientService
{
    Task<PatientResponse> Register(RegisterPatientRequest request, CancellationToken cancellationToken = default);
}

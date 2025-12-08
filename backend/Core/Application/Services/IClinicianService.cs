using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;

namespace Core.Application.Services;

public interface IClinicianService
{
    Task<ClinicianResponse> Register(RegisterClinicianRequest request, CancellationToken cancellationToken = default);
}

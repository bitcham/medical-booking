using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;

namespace Core.Application.Services;

public interface IClinicianService
{
    Task<ClinicianResponse> Register(RegisterClinicianRequest request, CancellationToken cancellationToken = default);
    Task<ClinicianResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClinicianResponse> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClinicianResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}

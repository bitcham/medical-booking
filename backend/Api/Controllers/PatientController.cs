using System.Security.Claims;
using Core.Application.Dtos.Responses;
using Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
public class PatientController(IPatientService patientService) : ControllerBase
{
    [HttpGet(ApiEndpoints.Patients.GetById)]
    [Authorize]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PatientResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var patient = await patientService.GetByIdAsync(id, cancellationToken);
        return Ok(patient);
    }

    [HttpGet(ApiEndpoints.Patients.GetMe)]
    [Authorize]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PatientResponse>> GetMe(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue("sub")!);
        var patient = await patientService.GetByUserIdAsync(userId, cancellationToken);
        return Ok(patient);
    }
}

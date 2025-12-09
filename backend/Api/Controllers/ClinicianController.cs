using System.Security.Claims;
using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;
using Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
public class ClinicianController(
    IClinicianService clinicianService,
    ITimeSlotService timeSlotService
) : ControllerBase
{
    [HttpGet(ApiEndpoints.Clinicians.GetAll)]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ClinicianResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClinicianResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var clinicians = await clinicianService.GetAllAsync(cancellationToken);
        return Ok(clinicians);
    }

    [HttpGet(ApiEndpoints.Clinicians.GetById)]
    [Authorize]
    [ProducesResponseType(typeof(ClinicianResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClinicianResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var clinician = await clinicianService.GetByIdAsync(id, cancellationToken);
        return Ok(clinician);
    }

    [HttpGet(ApiEndpoints.Clinicians.GetMe)]
    [Authorize]
    [ProducesResponseType(typeof(ClinicianResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClinicianResponse>> GetMe(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue("sub")!);
        var clinician = await clinicianService.GetByUserIdAsync(userId, cancellationToken);
        return Ok(clinician);
    }

    [HttpPost(ApiEndpoints.Clinicians.GenerateTimeSlots)]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<TimeSlotResponse>), StatusCodes.Status201Created)]
    public async Task<ActionResult<IEnumerable<TimeSlotResponse>>> GenerateTimeSlots(
        Guid id, 
        GenerateTimeSlotsRequest request, 
        CancellationToken cancellationToken)
    {
        var slots = await timeSlotService.GenerateSlotsAsync(id, request, cancellationToken);
        return CreatedAtAction(nameof(GetTimeSlots), new { id, version = "1.0" }, slots);
    }

    [HttpGet(ApiEndpoints.Clinicians.GetTimeSlots)]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<TimeSlotResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TimeSlotResponse>>> GetTimeSlots(
        Guid id, 
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        if (date.HasValue)
        {
            var availableSlots = await timeSlotService.GetAvailableByClinicianIdAsync(id, date.Value, cancellationToken);
            return Ok(availableSlots);
        }
        
        var slots = await timeSlotService.GetByClinicianIdAsync(id, cancellationToken);
        return Ok(slots);
    }
}


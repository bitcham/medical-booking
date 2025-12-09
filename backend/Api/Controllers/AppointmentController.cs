using System.Security.Claims;
using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;
using Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
public class AppointmentController(
    IAppointmentService appointmentService,
    IPatientService patientService,
    IClinicianService clinicianService
) : ControllerBase
{
    [HttpPost(ApiEndpoints.Appointments.Create)]
    [Authorize]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<AppointmentResponse>> Create(
        CreateAppointmentRequest request, 
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue("sub")!);
        var patient = await patientService.GetByUserIdAsync(userId, cancellationToken);
        
        var appointment = await appointmentService.CreateAsync(patient.Id, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = appointment.Id, version = "1.0" }, appointment);
    }

    [HttpGet(ApiEndpoints.Appointments.GetAll)]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue("sub")!);
        var role = User.FindFirstValue("role");

        if (role == "Patient")
        {
            var patient = await patientService.GetByUserIdAsync(userId, cancellationToken);
            var appointments = await appointmentService.GetByPatientIdAsync(patient.Id, cancellationToken);
            return Ok(appointments);
        }
        else
        {
            // For clinicians, get clinician by userId first, then get appointments
            var clinician = await clinicianService.GetByUserIdAsync(userId, cancellationToken);
            var appointments = await appointmentService.GetByClinicianIdAsync(clinician.Id, cancellationToken);
            return Ok(appointments);
        }
    }

    [HttpGet(ApiEndpoints.Appointments.GetById)]
    [Authorize]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AppointmentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await appointmentService.GetByIdAsync(id, cancellationToken);
        return Ok(appointment);
    }

    [HttpPut(ApiEndpoints.Appointments.Cancel)]
    [Authorize]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AppointmentResponse>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await appointmentService.CancelAsync(id, cancellationToken);
        return Ok(appointment);
    }

    [HttpPut(ApiEndpoints.Appointments.Confirm)]
    [Authorize]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AppointmentResponse>> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await appointmentService.ConfirmAsync(id, cancellationToken);
        return Ok(appointment);
    }

    [HttpPut(ApiEndpoints.Appointments.Reschedule)]
    [Authorize]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AppointmentResponse>> Reschedule(
        Guid id, 
        RescheduleAppointmentRequest request, 
        CancellationToken cancellationToken)
    {
        var appointment = await appointmentService.RescheduleAsync(id, request, cancellationToken);
        return Ok(appointment);
    }
}

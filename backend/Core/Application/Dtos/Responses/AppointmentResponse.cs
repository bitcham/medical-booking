using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Application.Dtos.Responses;

public record AppointmentResponse(
    Guid Id,
    Guid PatientId,
    string PatientName,
    Guid ClinicianId,
    string ClinicianName,
    string Specialization,
    TimeSlotResponse TimeSlot,
    AppointmentStatus Status,
    string? Notes,
    DateTimeOffset CreatedAt
)
{
    public static AppointmentResponse FromEntity(Appointment appointment)
    {
        return new AppointmentResponse(
            appointment.Id,
            appointment.PatientId,
            $"{appointment.Patient.User.FirstName} {appointment.Patient.User.LastName}",
            appointment.ClinicianId,
            $"{appointment.Clinician.User.FirstName} {appointment.Clinician.User.LastName}",
            appointment.Clinician.Specialization,
            TimeSlotResponse.FromEntity(appointment.TimeSlot),
            appointment.Status,
            appointment.Notes,
            appointment.CreatedAt
        );
    }
}
